Shader "Unlit/AlexeySkyShader"
{
    Properties
    {
        _ColorA ("ColorA", Color) = (1, 1, 1, 1)
        _ColorB ("ColorB", Color) = (1, 1, 1, 1)
        _ColorC ("ColorC", Color) = (1, 1, 1, 1)
        _ColorD ("ColorD", Color) = (1, 1, 1, 1)
        _ColorE ("ColorE", Color) = (1, 1, 1, 1)
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull front

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _ColorA;
            float4 _ColorB;
            float4 _ColorC;
            float4 _ColorD;
            float4 _ColorE;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                float j = 1 - i.uv.y;
                float4 _NewCol = lerp(_ColorA, _ColorB, 1 - j);
                if (j > 0 && j < 0.4) _NewCol = lerp(_ColorA, _ColorB, 1 - j / 0.4);
                if (j > 0.4 && j < 0.5) _NewCol = lerp(_ColorB, _ColorC, 1 - (0.5 - j) / 0.1);
                if (j > 0.5 && j < 0.6) _NewCol = lerp(_ColorC, _ColorD, 1 - (0.6 - j) / 0.1);
                if (j > 0.6 && j < 1) _NewCol = lerp(_ColorD, _ColorE, 1 - (1 - j) / 0.4);
                return _NewCol;
            }
            ENDCG
        }
    }
}
