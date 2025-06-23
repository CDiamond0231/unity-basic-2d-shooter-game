// @brief Evaluates a smooth function f(x) with specified boundary conditions.
// 
// Conditions:
//  float f( float x, float dx0, float dx1 ) { ?... }
//  f(0)=0
//  f(1)=1
//  f'(0)=dx0 on the right side of 0
//  f'(1)=dx1 on the left side of 1
//  f(x) is smooth for x in [0,1]
//  f(x) = 0 for x < 0
//  f(x) = 1 for x > 1
//
// 
// This version avoids explicit 'if' statements for better GPU performance
// by using arithmetic operations like step() and lerp().
// 
// @param x   The input value.
// @param dx0 The desired derivative (slope) at x=0.
// @param dx1 The desired derivative (slope) at x=1.
// @return The interpolated float value.
Shader "Custom/HermiteBlendGradient"
{
    Properties
    {
        _X ("Input X", Range(-0.5, 1.5)) = 0.5
        _Dx0 ("Derivative at 0", Float) = 0.0
        _Dx1 ("Derivative at 1", Float) = -0.5
        _ColourA ("Gradient Start Colour", Color) = (0,0,0,1)
        _ColourB ("Gradient End Colour", Color) = (1,1,1,1)
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


            // Uniforms ~~~~~
            float _X;
            float _Dx0;
            float _Dx1;
            fixed4 _ColourA;
            fixed4 _ColourB;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            float HermiteBlend(float x, float dx0, float dx1)
            {
                // P(x)=a*x3+b*x2+c*x+d
                // a=dx0+dx1-2
                // b=3-2dx0-dx1
                // c=dx0
                // d=0
                float a = dx0 + dx1 - 2.0;
                float b = 3.0 - 2.0 * dx0 - dx1;
                float p = a * x * x * x + b * x * x + dx0 * x;
                float result = lerp(0.0, p, step(0.0, x)); // Handles x < 0
                result = lerp(result, 1.0, step(1.0, x));  // Handles x > 1
                return result;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float blendedVal = HermiteBlend(_X, _Dx0, _Dx1);
                fixed4 finalColour = lerp(_ColourA, _ColourB, blendedVal);
                return finalColour;
            }
            ENDCG
        }
    }
}
