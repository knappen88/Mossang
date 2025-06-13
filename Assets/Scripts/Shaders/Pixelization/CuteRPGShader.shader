Shader "Hidden/CuteRPGColorGrading"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Brightness ("Brightness", Float) = 0
        _Contrast ("Contrast", Float) = 1
        _Saturation ("Saturation", Float) = 1
        _Vibrance ("Vibrance", Range(0, 1)) = 0.5
        
        _TintColor ("Tint Color", Color) = (1,1,1,1)
        _TintStrength ("Tint Strength", Range(0, 1)) = 0.1
        _ShadowColor ("Shadow Color", Color) = (0.8,0.7,0.9,1)
        _HighlightColor ("Highlight Color", Color) = (1,1,0.9,1)
        
        _PastelStrength ("Pastel Strength", Range(0, 1)) = 0.3
        _BloomThreshold ("Bloom Threshold", Range(0, 1)) = 0.8
        
        _FinalTint ("Final Combined Tint", Color) = (1,1,1,1)
        _DayNightIntensity ("Day Night Intensity", Float) = 1
    }
    
    SubShader
    {
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
            float _Brightness;
            float _Contrast;
            float _Saturation;
            float _Vibrance;
            
            float4 _TintColor;
            float _TintStrength;
            float4 _ShadowColor;
            float4 _HighlightColor;
            
            float _PastelStrength;
            float _BloomThreshold;
            
            float4 _FinalTint;
            float _DayNightIntensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            // Функция для RGB в HSV
            float3 rgb2hsv(float3 c)
            {
                float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
                float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
                
                float d = q.x - min(q.w, q.y);
                float e = 1.0e-10;
                return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
            }
            
            // Функция для HSV в RGB
            float3 hsv2rgb(float3 c)
            {
                float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
            }
            
            // Smooth step для мягких переходов
            float smootherstep(float edge0, float edge1, float x)
            {
                x = clamp((x - edge0) / (edge1 - edge0), 0.0, 1.0);
                return x * x * x * (x * (x * 6 - 15) + 10);
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Сохраняем оригинальный альфа канал
                float originalAlpha = col.a;
                
                // Применяем яркость
                col.rgb += _Brightness * _DayNightIntensity;
                
                // Применяем контраст
                col.rgb = ((col.rgb - 0.5) * _Contrast) + 0.5;
                
                // Расчет luminance для различных эффектов
                float lum = dot(col.rgb, float3(0.299, 0.587, 0.114));
                
                // Vibrance (более умная насыщенность)
                float3 intensity = float3(lum, lum, lum);
                float sat = 1.0 + (_Vibrance * (1.0 - (max(col.r, max(col.g, col.b)) - min(col.r, min(col.g, col.b)))));
                col.rgb = lerp(intensity, col.rgb, sat);
                
                // Обычная насыщенность
                col.rgb = lerp(lum, col.rgb, _Saturation);
                
                // Shadow/Highlight тонирование
                float shadowMask = 1.0 - smootherstep(0.0, 0.5, lum);
                float highlightMask = smootherstep(0.5, 1.0, lum);
                
                col.rgb = lerp(col.rgb, col.rgb * _ShadowColor.rgb, shadowMask * 0.5);
                col.rgb = lerp(col.rgb, col.rgb * _HighlightColor.rgb, highlightMask * 0.3);
                
                // Pastel эффект (смягчение цветов)
                float3 pastelColor = col.rgb;
                pastelColor = pow(pastelColor, 0.6); // Поднимаем темные тона
                pastelColor = lerp(pastelColor, float3(1, 1, 1), 0.2); // Добавляем белого
                col.rgb = lerp(col.rgb, pastelColor, _PastelStrength);
                
                // Применяем общий тинт
                col.rgb = lerp(col.rgb, col.rgb * _FinalTint.rgb, _TintStrength);
                
                // Soft bloom effect для ярких областей
                if (lum > _BloomThreshold)
                {
                    float bloomStrength = (lum - _BloomThreshold) / (1.0 - _BloomThreshold);
                    col.rgb += col.rgb * bloomStrength * 0.3;
                }
                
                // Немного увеличиваем яркость для cute стиля
                col.rgb = pow(col.rgb, 0.95);
                
                // Клампим значения
                col.rgb = clamp(col.rgb, 0.0, 1.0);
                
                // Восстанавливаем альфа канал
                col.a = originalAlpha;
                
                return col;
            }
            ENDCG
        }
    }
}