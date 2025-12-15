Shader "Unlit/Funnel"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Curve1 ("First Curve", Range(0.5, 5)) = 1.5
        _Curve2 ("Second Curve", Range(1, 6)) = 3.0
        _EndWidth ("End Width", Range(0, 0.3)) = 0.05
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float4 _Color;
            float _Curve1;
            float _Curve2;
            float _EndWidth;

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float x = i.uv.x;
                float y = abs(i.uv.y - 0.5);

                float t = 1.0 - x;

                float curve1 = pow(t, _Curve1);
                float curve2 = pow(curve1, _Curve2);

                float halfWidth = lerp(0.5, _EndWidth, 1.0 - curve2);

                if (y > halfWidth)
                    discard;

                return _Color;
            }
            ENDCG
        }
    }
}
