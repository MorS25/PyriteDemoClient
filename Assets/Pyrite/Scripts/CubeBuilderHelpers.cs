﻿namespace Pyrite
{
    using System;
    using System.Globalization;
    using UnityEngine;

    public static class CubeBuilderHelpers
    {
        /* OBJ file tags */
        private const string O = "o";
        private const string G = "g";
        private const string V = "v";
        private const string Vt = "vt";
        private const string Vn = "vn";
        private const string F = "f";
        private const string Mtl = "mtllib";
        private const string Uml = "usemtl";

        private static readonly Color DefaultAmbient = new Color(0.2f, 0.2f, 0.2f);
        private static readonly Color DefaultDiffuse = new Color(0.8f, 0.8f, 0.8f);
        private static readonly Color DefaultSpecular = new Color(1.0f, 1.0f, 1.0f);

        private const float DefaultAlplha = 1.0f;
        private const int DefaultIllumType = 2;
        private const float DefaultShininess = 0f;

        private static readonly Shader UnlitShader = Shader.Find("Unlit/Texture");

        public static MaterialData GetDefaultMaterialData(int x, int y, int lod)
        {
            var current = new MaterialData();

            // newmtl material_0
            current.X = x;
            current.Y = y;
            current.Lod = lod;

            // Ka 0.200000 0.200000 0.200000
            current.Ambient = DefaultAmbient; // Gc(new[] {"Ka", "0.200000", "0.200000", "0.200000"});

            // Kd 0.800000 0.800000 0.800000
            current.Diffuse = DefaultDiffuse; // Gc(new[] {"Kd", "0.800000", "0.800000", "0.800000"});

            // Ks 1.000000 1.000000 1.000000
            current.Specular = DefaultSpecular; // Gc(new[] {"Ks", "1.000000", "1.000000", "1.000000"});

            // Tr 1.000000
            current.Alpha = DefaultAlplha; // Cf("1.000000");

            // illum 2
            current.IllumType = DefaultIllumType; // Ci("2");

            // Ns 0.000000
            current.Shininess = DefaultShininess; // Cf("0.000000")/1000;

            // map_Kd model.jpg
            current.DiffuseTexPath = "model.jpg";

            return current;
        }

        private static float Cf(string v)
        {
            return Convert.ToSingle(v.Trim(), new CultureInfo("en-US"));
        }

        private static int Ci(string v)
        {
            return Convert.ToInt32(v.Trim(), new CultureInfo("en-US"));
        }

        private static Color Gc(string[] p)
        {
            return new Color(Cf(p[1]), Cf(p[2]), Cf(p[3]));
        }

        public static Material GetMaterial(bool useUnlitShader, MaterialData md)
        {
            Material m;
            // Use an unlit shader for the model if set
            if (useUnlitShader)
            {
                m = new Material(UnlitShader);
            }
            else
            {
                if (md.IllumType == 2)
                {
                    m = new Material(Shader.Find("Specular"));
                    m.SetColor("_SpecColor", md.Specular);
                    m.SetFloat("_Shininess", md.Shininess);
                }
                else
                {
                    m = new Material(Shader.Find("Diffuse"));
                }

                m.SetColor("_Color", md.Diffuse);
            }

            if (md.DiffuseTex != null)
            {
                m.SetTexture("_MainTex", md.DiffuseTex);
            }


            return m;
        }
    }
}