using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BasicUnity2DShooter.Utilities
{
    public static class BezierSpline
    {
        /// <summary> Evaluates a point on a Cubic Bezier curve at a given time 't'.
        /// The curve is defined by four control points: _pointStart, _controlPoint1, _controlPoint2, _endPoint (p0, p1, p2, p3).
        /// Math for this came from: https://blog.maximeheckel.com/posts/cubic-bezier-from-math-to-motion/ </summary>
        /// <returns> The Vector3 point on the Bezier curve at parameter 't'. </returns>
        public static Vector3 GetPointOnCubicBezierCurve(Vector3 _pointStart, Vector3 _tangentPoint1,
                                                            Vector3 _tangentPoint2, Vector3 _endPoint,
                                                            float _t)
        {
            if (_t< 0.0f)
            {
                _t = 0.0f;
            }
            else if (_t > 1.0f)
            {
                _t = 1.0f;
            }

            // Calculate the coefficients.
            float tSqr = _t * _t;
            float invertedT = 1.0f - _t;
            float invertedTSqr = invertedT * invertedT;

            float b0 = invertedTSqr * invertedT;    // (1-t)^3
            float b1 = 3.0f * invertedTSqr * _t;    // 3 * (1-t)^2 * t
            float b2 = 3.0f * invertedT * tSqr;     // 3 * (1-t) * t^2
            float b3 = tSqr * _t;                   // t^3

            // Apply the formula: B(t) = b0*P0 + b1*P1 + b2*P2 + b3*P3
            Vector3 result = (_pointStart * b0)
                + (_tangentPoint1 * b1)
                + (_tangentPoint2 * b2)
                + (_endPoint * b3);

            return result;
        }


        /// <summary> This function chains multiple Cubic Bezier curves to interpolate an arbitrary number of points.
        /// Math functionality came from: https://apoorvaj.io/cubic-bezier-through-four-points/ </summary>
        public static Vector3 GetPointOnInterpolatedBezierSpline(List<Vector3> _points, float _globalTime)
        {
            int numPoints = _points.Count;
            if (numPoints == 0)
            {
                Debug.LogError("Error: Cannot interpolate an empty set of points.");
                return Vector3.zero;
            }
            if (numPoints == 1)
            {
                return _points[0];
            }

            if (_globalTime <= 0.0f)
            {
                return _points[0];
            }
            else if (_globalTime >= 1.0f)
            {
                return _points[numPoints - 1];
            }

            int numSegments = numPoints - 1;
            float segmentSplit = _globalTime * numSegments;
            int segmentIndex = (int)segmentSplit;

            if (segmentIndex >= numSegments)
            {
                segmentIndex = numSegments - 1;
            }

            float localT = segmentSplit - segmentIndex;

            Vector3 nowPoint = _points[segmentIndex];
            Vector3 nextPoint = _points[segmentIndex + 1];

            Vector3 tangentPoint1;
            if (segmentIndex == 0)
            {
                tangentPoint1 = (_points[1] - _points[0]);
            }
            else
            {
                tangentPoint1 = (_points[segmentIndex + 1] - _points[segmentIndex - 1]) * 0.5f;
            }

            Vector3 tangentPoint2;
            if ((segmentIndex + 1) == (numPoints - 1))
            {
                tangentPoint2 = (_points[numPoints - 1] - _points[numPoints - 2]);
            }
            else
            {
                tangentPoint2 = (_points[segmentIndex + 2] - _points[segmentIndex]) * 0.5f;
            }

            Vector3 C0 = nowPoint;
            Vector3 C1 = nowPoint + (tangentPoint1 * 0.8f);
            Vector3 C2 = nextPoint - (tangentPoint2 * 0.8f);
            Vector3 C3 = nextPoint;

            return GetPointOnCubicBezierCurve(C0, C1, C2, C3, localT);
        }

        /// <summary> This function chains multiple Cubic Bezier curves to interpolate an arbitrary number of points.
        /// Math functionality came from: https://apoorvaj.io/cubic-bezier-through-four-points/ </summary>
        public static Vector3 GetPointOnInterpolatedBezierSpline(Vector3[] _points, float _globalTime)
        {
            int numPoints = _points.Length;
            if (numPoints == 0)
            {
                Debug.LogError("Error: Cannot interpolate an empty set of points.");
                return Vector3.zero;
            }
            if (numPoints == 1)
            {
                return _points[0];
            }

            if (_globalTime <= 0.0f)
            {
                return _points[0];
            }
            else if (_globalTime >= 1.0f)
            {
                return _points[numPoints - 1];
            }

            int numSegments = numPoints - 1;
            float segmentSplit = _globalTime * numSegments;
            int segmentIndex = (int)segmentSplit;

            if (segmentIndex >= numSegments)
            {
                segmentIndex = numSegments - 1;
            }

            float localT = segmentSplit - segmentIndex;

            Vector3 nowPoint = _points[segmentIndex];
            Vector3 nextPoint = _points[segmentIndex + 1];

            Vector3 tangentPoint1;
            if (segmentIndex == 0)
            {
                tangentPoint1 = (_points[1] - _points[0]);
            }
            else
            {
                tangentPoint1 = (_points[segmentIndex + 1] - _points[segmentIndex - 1]) * 0.5f;
            }

            Vector3 tangentPoint2;
            if ((segmentIndex + 1) == (numPoints - 1))
            {
                tangentPoint2 = (_points[numPoints - 1] - _points[numPoints - 2]);
            }
            else
            {
                tangentPoint2 = (_points[segmentIndex + 2] - _points[segmentIndex]) * 0.5f;
            }

            Vector3 C0 = nowPoint;
            Vector3 C1 = nowPoint + (tangentPoint1 * 0.8f);
            Vector3 C2 = nextPoint - (tangentPoint2 * 0.8f);
            Vector3 C3 = nextPoint;

            return GetPointOnCubicBezierCurve(C0, C1, C2, C3, localT);
        }
    }
}
