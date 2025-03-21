Shader "Unlit/GradientBackground"
{
    Properties
    {
        _ColorLeft ("Cor Esquerda", Color) = (0,0,0,1) 
        _ColorRight ("Cor Direita", Color) = (1,1,1,1) 
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            fixed4 _ColorLeft;
            fixed4 _ColorRight;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {

            fixed4 col = lerp(_ColorLeft, _ColorRight, i.uv.x);
    
  
            float noise = frac(sin(dot(i.uv.xy, float2(12.9898, 78.233))) * 43758.5453);
            noise = (noise - 0.5) * 0.002;
    
            col.rgb += noise;
            return col;
            }
            ENDCG
        }
    }
}