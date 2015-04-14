﻿using System.Runtime.InteropServices;

namespace Assets.Cube_Loader.src
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Assets.Cube_Loader.Extensions;
    using SimpleJSON;
    using UnityEngine;


    public class PyriteQuery
    {
        private readonly string apiUrl = "http://az744221.vo.msecnd.net/";

        private PyriteSet _set = null;

        public PyriteSet Set
        {
            get
            {
                if (!Loaded || _set == null)
                {
                    throw new InvalidOperationException(
                        "PyriteQuery has not been loaded yet. You must call Load and wait for it to finish before accessing Set data.");
                }
                else
                {
                    return _set;
                }
            }
            private set { _set = value; }
        }

        public string SetName { get; private set; }
        public string Version { get; private set; }

        public bool Loaded { get; private set; }

        private const string StatusKey = "status";
        private const string ResultKey = "result";
        private const string NameKey = "name";
        private const string DetailLevelsKey = "detailLevels";
        private const string SetSizeKey = "setSize";
        private const string TextureSetSizeKey = "textureSetSize";
        private const string WorldBoundsKey = "worldBounds";
        private const string WorldCubeScaleKey = "worldCubeScale";
        private const string MaxKey = "max";
        private const string MinKey = "min";
        private const string XKey = "x";
        private const string YKey = "y";
        private const string ZKey = "z";
        private const string OKValue = "OK";

        public PyriteQuery(string setName, string version, string apiUrl = null)
        {
            if (!string.IsNullOrEmpty(apiUrl))
            {
                Debug.Log("Overriding PyriteServer url to " + apiUrl);
                this.apiUrl = apiUrl;
            }
            SetName = setName;
            Version = version;
            Loaded = false;
        }

        public string GetModelPath(int lod, int x, int y, int z)
        {
            return GetModelPath(GetLODKey(lod), x, y, z);
        }

        public string GetModelPath(string lod, int x, int y, int z)
        {
            return string.Format("{0}/sets/{1}/{2}/models/{3}/{4},{5},{6}", apiUrl, SetName, Version, lod, x, y, z);
        }

        public string GetTexturePath(int lod, int x, int y)
        {
            return GetTexturePath(GetLODKey(lod), x, y);
        }

        public string GetTexturePath(string lod, int x, int y)
        {
            return string.Format("{0}/sets/{1}/{2}/textures/{3}/{4},{5}", apiUrl, SetName, Version, lod, x, y);
        }

        public string GetLODKey(int lod)
        {
            return "L" + lod;
        }

        public Vector3 GetNextCubeFactor(int lod)
        {
            var currentSetSize = this.Set.Version.DetailLevels[lod].SetSize;
            if (!this.Set.Version.DetailLevels.ContainsKey(lod - 1))
            {
                return Vector3.one;
            }

            var nextSetSize = this.Set.Version.DetailLevels[lod-1].SetSize;
            return new Vector3(
                nextSetSize.x / currentSetSize.x,
                nextSetSize.y / currentSetSize.y,
                nextSetSize.z / currentSetSize.z
                );
        }

        private int GetDetailNumberFromName(string levelName)
        {
            return int.Parse(levelName.Substring(1));
        }

        public IEnumerator Load()
        {
            Debug.Log("CubeQuery started against: " + apiUrl);
            WWW loader = null;
            var set = new PyriteSet() { Name = SetName };
            Set = set;
            var setUrl = apiUrl + "/sets/" + SetName+ "/";
            loader = WWWExtensions.CreateWWW(path: setUrl);
            yield return loader;
            Debug.Log(loader.GetDecompressedText());
            var parsedContent = JSON.Parse(loader.GetDecompressedText());
            if (!parsedContent[StatusKey].Value.Equals(OKValue))
            {
                Debug.LogError("Failure getting set info for " + set.Name);
                yield break;
            }
            var version = new PyriteSetVersion() { Name = Version, Set = set};
            set.Version = version;
            string versionUrl = setUrl + Version + "/";
            loader = WWWExtensions.CreateWWW(versionUrl);
            yield return loader;
            parsedContent = JSON.Parse(loader.GetDecompressedText());
            if (!parsedContent[StatusKey].Value.Equals(OKValue))
            {
                Debug.LogError("Failure getting set version info for " + set.Name + " - " + Version);
                yield break;
            }
            var parsedDetailLevels = parsedContent[ResultKey][DetailLevelsKey].AsArray;
            for (int k = 0; k < parsedDetailLevels.Count; k++)
            {
                var detailLevel = new PyriteSetVersionDetailLevel()
                {
                    Name = parsedDetailLevels[k][NameKey],
                    Version = version
                };
                version.DetailLevels[GetDetailNumberFromName(detailLevel.Name)] = detailLevel;
                detailLevel.SetSize = new Vector3(
                    parsedDetailLevels[k][SetSizeKey][XKey].AsFloat,
                    parsedDetailLevels[k][SetSizeKey][YKey].AsFloat,
                    parsedDetailLevels[k][SetSizeKey][ZKey].AsFloat
                    );

                detailLevel.TextureSetSize = new Vector2(
                    parsedDetailLevels[k][TextureSetSizeKey][XKey].AsFloat,
                    parsedDetailLevels[k][TextureSetSizeKey][YKey].AsFloat
                    );

                detailLevel.WorldBoundsMax = new Vector3(
                    parsedDetailLevels[k][WorldBoundsKey][MaxKey][XKey].AsFloat,
                    parsedDetailLevels[k][WorldBoundsKey][MaxKey][YKey].AsFloat,
                    parsedDetailLevels[k][WorldBoundsKey][MaxKey][ZKey].AsFloat
                    );

                detailLevel.WorldBoundsMin = new Vector3(
                    parsedDetailLevels[k][WorldBoundsKey][MinKey][XKey].AsFloat,
                    parsedDetailLevels[k][WorldBoundsKey][MinKey][YKey].AsFloat,
                    parsedDetailLevels[k][WorldBoundsKey][MinKey][ZKey].AsFloat
                    );

                detailLevel.WorldCubeScale = new Vector3(
                    parsedDetailLevels[k][WorldCubeScaleKey][XKey].AsFloat,
                    parsedDetailLevels[k][WorldCubeScaleKey][YKey].AsFloat,
                    parsedDetailLevels[k][WorldCubeScaleKey][ZKey].AsFloat);

                detailLevel.WorldBoundsSize =
                    detailLevel.WorldBoundsMax -
                    detailLevel.WorldBoundsMin;

                var maxBoundingBoxQuery = string.Format("{0},{1},{2}/{3},{4},{5}",
                    detailLevel.WorldBoundsMin.x,
                    detailLevel.WorldBoundsMin.y,
                    detailLevel.WorldBoundsMin.z,
                    detailLevel.WorldBoundsMax.x,
                    detailLevel.WorldBoundsMax.y,
                    detailLevel.WorldBoundsMax.z
                    );

                var cubesUrl = versionUrl + "query/" + detailLevel.Name + "/" +
                                maxBoundingBoxQuery;

                loader = WWWExtensions.CreateWWW(cubesUrl);
                yield return loader;
                parsedContent = JSON.Parse(loader.GetDecompressedText());
                if (!parsedContent[StatusKey].Value.Equals(OKValue))
                {
                    Debug.LogError("Failure getting cube query against: " + cubesUrl);
                    yield break;
                }

                var parsedCubes = parsedContent[ResultKey].AsArray;
                detailLevel.Cubes = new PyriteCube[parsedCubes.Count];
                for (int l = 0; l < detailLevel.Cubes.Length; l++)
                {
                    detailLevel.Cubes[l] = new PyriteCube()
                    {
                        X = parsedCubes[l][0].AsInt,
                        Y = parsedCubes[l][1].AsInt,
                        Z = parsedCubes[l][2].AsInt,
                    };
                }
            }

            Loaded = true;
        }
    }

    public class PyriteCube : IEquatable<PyriteCube>
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public bool Equals(PyriteCube other)
        {
            return null != other && other.X == this.X && other.Y == this.Y && other.Z == this.Z;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PyriteCube);
        }
    }

    public class PyriteSetVersion
    {
        public string Name { get; set; }
        public PyriteSet Set { get; set; }

        public Dictionary<int, PyriteSetVersionDetailLevel> DetailLevels { get; private set; }

        public PyriteSetVersion()
        {
            DetailLevels = new Dictionary<int, PyriteSetVersionDetailLevel>();
        }
    }

    public class PyriteSetVersionDetailLevel
    {
        public string Name { get; set; }
        public PyriteSetVersion Version { get; set; }
        public Vector3 SetSize { get; set; }
        public Vector2 TextureSetSize { get; set; }
        public Vector3 WorldBoundsMax { get; set; }
        public Vector3 WorldBoundsMin { get; set; }
        public Vector3 WorldBoundsSize { get; set; }
        public Vector3 WorldCubeScale { get; set; }
        public PyriteCube[] Cubes { get; set; }

        public Vector2 TextureCoordinatesForCube(float cubeX, float cubeY)
        {
            int textureXPosition = (int)(cubeX / (SetSize.x / TextureSetSize.x));
            int textureYPosition = (int)(cubeY / (SetSize.y / TextureSetSize.y));
            return new Vector2(textureXPosition, textureYPosition);
        }

        // Returns the center of the cube (point at the middle of each axis distance) in world space
        public Vector3 GetWorldCoordinatesForCube(PyriteCube cube)
        {
            float xPos = WorldBoundsMin.x + WorldCubeScale.x * cube.X +
                         WorldCubeScale.x * 0.5f;
            float yPos = WorldBoundsMin.y + WorldCubeScale.y * cube.Y +
                         WorldCubeScale.y * 0.5f;
            float zPos = WorldBoundsMin.z + WorldCubeScale.z * cube.Z +
                         WorldCubeScale.z * 0.5f;
            return new Vector3(xPos, yPos, zPos);
        }
    }

    public class PyriteSet
    {
        public string Name { get; set; }

        public PyriteSetVersion Version { get; set; }

        public PyriteSet()
        {
        }
    }
}
