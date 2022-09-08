// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "FMVTexture" {
    Properties {
        _MainTex ("Luminance Texture", 2D) = "black" {} // for the black result
        _ChromaTex ("Chroma Texture", 2D) = "green" {} // for the monochrome result
    }
    SubShader {
        Pass{
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _ChromaTex;
            uniform float4 _MainTex_ST;

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord,_MainTex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 luminance = tex2D(_MainTex, i.uv);
                float4 chrominance = tex2D(_ChromaTex, i.uv);
                float4 color;

                if (i.uv.x < 0.0f || i.uv.y < 0.0f || i.uv.x > 1.0f || i.uv.y > 1.0f) {
                    color = float4(1.0f, 0.0f, 0.0f, 1.0f);
                } else {
                    float3 ycbcr =
                        float3(luminance.x - 0.0625,
                            chrominance.x - 0.5,
                            chrominance.y - 0.5);

                    color =
                        float4( dot(float3(1.1644f, 0.0f, 1.7927f), ycbcr), // R
                            dot(float3(1.1644f, -0.2133f, -0.5329f), ycbcr), // G
                            dot(float3(1.1644f, 2.1124f, 0.0f), ycbcr), // B
                            1.0f );

                    // convert linear to sRGB
#ifndef UNITY_COLORSPACE_GAMMA
                    float4x4 Lin2sRGB = float4x4(3.2406f,-1.5372,-0.4986,0,     -0.9689, 1.8758,  0.0415,0,      0.0557,-0.2040,1.0570,  0,   0,0,0,1);
                    float4x4 trans = float4x4(0.4124, 0.3576, 0.1805,0,     0.2126,0.7152,0.0722,0,      0.0193,0.1192,0.9502,0,   0,0,0,1);
                    color = mul(Lin2sRGB, color);
                    color = mul(trans, color);
                    color.rgb = pow(color.rgb, 2.2);
#endif
                }

                float4 ret = color;
                return ret;
            }

            ENDCG

        }
    }
}
