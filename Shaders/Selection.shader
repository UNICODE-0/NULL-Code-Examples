Shader "Unlit/Selection"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _FlashingSpeed ("Flashing speed", Range(0,10)) = 0.8
        _AlphaOffset ("Alpha offset", Range(0,1)) = 0.7

    }
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        Pass
        {
            Cull off
            Zwrite off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            float4 _Color;
            float _FlashingSpeed;
            float _AlphaOffset;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv0 : TEXCOORD0;
            };

            struct v2f
            {
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv0;
                o.normal = v.normal;

                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                bool IsUpOrDown = abs(i.normal.y) < 0.999; // cut up and down
                
                float Flashing = abs(sin(_Time.y * _FlashingSpeed)) * _AlphaOffset + (1 - _AlphaOffset);
                fixed4 col = fixed4(_Color.r, _Color.g,_Color.b, _Color.a * i.uv.y * Flashing)  * IsUpOrDown;

                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
