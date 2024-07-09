Shader "Custom/CustomLightShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

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
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            fixed4 _Color;

            StructuredBuffer<float3> _CustomLightBuffer;
            uint _CustomLightCount;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float intensity = 0;
                for (uint idx = 0; idx < _CustomLightCount; idx++)
                {
                    intensity += 16 -  distance(_CustomLightBuffer[idx], i.worldPos);
                }
                return _Color * intensity;
            }
            ENDCG
        }
    }
}