Shader "Skybox/NightDay Cubemap"
{
    Properties
    {
        _Tex1("Cubemap 1", Cube) = "white" {}
        _Tex2("Cubemap 2", Cube) = "white" {}
        _Blend("Blend", Range(0, 1)) = 0.5
        _Rotation("Rotation", Range(0, 360)) = 0
        _RotationAxis("Rotation Axis", Vector) = (0, 1, 0, 0)
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float3 texcoord : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            samplerCUBE _Tex1;
            samplerCUBE _Tex2;
            float       _Blend;
            float       _Rotation;
            float4 _RotationAxis;

            float3 RotateAroundAxis(float3 dir, float3 axis, float degrees)
            {
                float rad = degrees * (UNITY_PI / 180.0);
                float sinR, cosR;
                sincos(rad, sinR, cosR);
                axis = normalize(axis);

                // Rodrigues' rotation formula
                return dir * cosR
                     + cross(axis, dir) * sinR
                     + axis * dot(axis, dir) * (1 - cosR);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.texcoord = RotateAroundAxis(v.vertex.xyz, _RotationAxis.xyz, _Rotation);
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 colorTex1 = texCUBE(_Tex1, i.texcoord);
                fixed4 colorTex2 = texCUBE(_Tex2, i.texcoord);
                return lerp(colorTex1, colorTex2, _Blend);
            }
            ENDCG
        }
    }
}