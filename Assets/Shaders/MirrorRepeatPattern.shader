// @brief Produces a mirrored repeating pattern, with identity behavior in a core interval.
// 
// Conditions:
// - The function is identity when x is in interval [a,b]
// - The function is mirrored repeated everywhere else:
//
// Segments outside [a,b] are reflections of the [a,b] interval itself.
// E.g. interval [b, b+(b-a)] maps back to [b, a].
// Interval [a-(b-a), a] maps back to [b, a].
// 
// @param x The input value.
// @param a The start of the identity interval [a, b].
// @param b The end of the identity interval [a, b].
// @return The mapped value.

Shader "Custom/MirrorRepeatPattern"
{
    Properties
    {
        _X ("Input X", Float) = 0.0
        _A ("Identity Start (A)", Float) = 0.0
        _B ("Identity End (B)", Float) = 1.0
        _ColourA ("Min Colour", Color) = (0.0, 0.0, 0.0, 1.0)
        _ColourB ("Max Colour", Color) = (1.0, 1.0, 1.0, 1.0)
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

            
            float _X;
            float _A;
            float _B;
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

            float mirror_repeat(float x, float a, float b)
            {
                float rangeLength = max(0.00001, b - a); // Prevents div by 0
                float xRelative = x - a;

                // Normalise rel to the Full Mirror Period [0-(2*range)]. 2*ramge for the mirror half.
                float twoRangeLength = 2.0 * rangeLength;
                float xRelativeNormalised = fmod(xRelative, twoRangeLength);
                xRelativeNormalised = xRelativeNormalised + twoRangeLength * step(xRelativeNormalised, 0.0); // Clamps to 0f<->1f

                // Determine which half-cycle we are in within the [0, twoRangeLength] period.
                //    If xRelativeNormalised is within [0, rangeLength], 'isMirroredHalf' is 0.0 (forward part).
                //    If xRelativeNormalised is within [rangeLength, twoRangeLength], 'isMirroredHalf' is 1.0 (mirrored part).
                float isMirroredHalf = step(rangeLength, xRelativeNormalised);
                float baseRangeValue = fmod(xRelativeNormalised, rangeLength);

                // Apply mirroring. Value is either baseRangeValue or (rangeLength - baseRangeValue) depending on isMirroredHalf value (either 0.0 or 1.0)
                // Translate the result back to the original offset (add 'a').
                float repeatResult = lerp(baseRangeValue, rangeLength - baseRangeValue, isMirroredHalf);
                repeatResult += a;

                // lerp will choose between the identity 'x' and the 'repeatResult'.
                float isWithinIdentityRange = step(a, x) * (1.0 - step(b, x));
                return lerp(repeatResult, x, isWithinIdentityRange);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float mirrorVal = mirror_repeat(_X, _A, _B);
                float normalisedVal = (mirrorVal - _A) / (_B - _A);
                normalisedVal = clamp(normalisedVal, 0.0, 1.0);
                fixed4 finalColour = lerp(_ColourA, _ColourB, normalisedVal);
                return finalColour;
            }
            ENDCG
        }
    }
}