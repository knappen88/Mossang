Shader "Hidden/PixelationEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PixelSize ("Pixel Size", Int) = 128
        _PixelationStrength ("Pixelation Strength", Range(0, 1)) = 1
        _AspectRatio ("Aspect Ratio", Float) = 1.777
        _PreserveAspect ("Preserve Aspect", Int) = 1
    }
    
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            int _PixelSize;
            float _PixelationStrength;
            float _AspectRatio;
            int _PreserveAspect;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                
                // Вычисляем размер пикселя
                float2 pixelSize = float2(1.0, 1.0) / float(_PixelSize);
                
                // Сохраняем соотношение сторон если нужно
                if (_PreserveAspect == 1)
                {
                    pixelSize.x = pixelSize.y * _AspectRatio;
                }
                
                // Пикселизация UV координат
                float2 pixelatedUV = floor(uv / pixelSize) * pixelSize + pixelSize * 0.5;
                
                // Интерполяция между оригинальным и пикселизированным изображением
                float2 finalUV = lerp(uv, pixelatedUV, _PixelationStrength);
                
                // Получаем цвет
                fixed4 col = tex2D(_MainTex, finalUV);
                
                return col;
            }
            ENDCG
        }
    }
}