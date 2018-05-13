using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace geniikw.DataRenderer2D
{
    [Serializable]
    public struct Spline : IEnumerable<Point>
    {
        public event Action EditCallBack;
        public MonoBehaviour SplineOwner;
        
        public void Initialize() {
            this = Default;
        }

        /// <summary>
        /// 중간에 p.position이 worldposition에서  localposition으로 바꿈.
        /// </summary>
        /// <param name="p"></param>
        public void Push(Point p)
        {
            if (mode == LineMode.BezierMode)
                throw new Exception("can't add");

            p.position = SplineOwner.transform.InverseTransformPoint(p.position);

            points.Add(p);

            if (EditCallBack != null)
                EditCallBack();
        }

        public void Push()
        {
            Push(Point.Zero);
        }


        public void Push(Vector3 worldPosition, Vector3 nextOffset, Vector3 prevOffset, float width)
        {
            Push(new Point(worldPosition, nextOffset, prevOffset, width));
        }

        public void EditPoint(int idx, Point p)
        {
            if(mode == LineMode.BezierMode &&( idx <0 || idx > 2))
            {
                throw new Exception("can't edit");
            }
            if (points.Count <= idx || idx < 0)
            {
                throw new Exception("can't edit" + points.Count + " " + idx);
            }

            p.position = SplineOwner.transform.InverseTransformPoint(p.position);
            if (mode == LineMode.BezierMode)
            {
                if (idx == 0)
                    pair.n0 = p;
                else
                    pair.n1 = p;
            }
            else
            {
                points[idx] = p;
            }

            if (EditCallBack != null)
                EditCallBack();
        }

        public void EditPoint(int idx, Vector3 worldPos, Vector3 nOffset, Vector3 pOffset, float width)
        {
            EditPoint(idx, new Point(worldPos, nOffset, pOffset, width));
        }

        public void EditPoint(int idx, Vector3 worldPos, float width)
        {
            EditPoint(idx,worldPos, Vector3.zero, Vector3.zero, width);
        }

        /// <summary>
        /// remove last point
        /// </summary>
        /// <returns></returns>
        public Point Pop()
        {
            if (mode == LineMode.BezierMode)
                throw new Exception("can't remove");

            var last = points[points.Count - 1];
            points.RemoveAt(points.Count - 1);

            if (EditCallBack != null)
                EditCallBack();

            return last;
        }
        
        public int Count
        {
            get
            {
                if (mode == LineMode.BezierMode)
                    return 2;
                return points.Count;
            }
        }

        public IEnumerable<Triple> TripleList
        {
            get
            {
                if (GetCount() < 2)
                    yield break;

                var mode = option.mode;
                var sr = option.startRatio;
                var er = option.endRatio;
                var color = option.color;
                            
                              
                var l = AllLength;
                var ls = sr * l;
                var le = er * l;
                var c = 0f;

                var fB = Point.Zero;
                var ff = true;
                var sB = Point.Zero;
                var sf = true;

                var index = 0;
                foreach(var p in TripleEnumerator())
                {                    
                    if (ff)
                    {
                        ff = false;
                        fB = p;
                        continue;
                    }
                    if (sf)
                    {
                        if (mode == LineOption.Mode.Loop && sr == 0f && er == 1f)
                            yield return new Triple(GetLastPoint(), GetFirstPoint(), p, color.Evaluate(0));

                        sf = false;
                        sB = p;
                        continue;
                    }
                    
                    c += CurveLength.Auto(fB, sB);
                    if (ls < c && c < le)
                    {
                        if (index == GetCount() - 1 && mode != LineOption.Mode.Loop)
                            break;
                        
                        yield return new Triple(fB, sB, p,color.Evaluate(c/l));
                    }
                    fB = sB;
                    sB = p;
                    index++;
                }
            }
        }

        public struct Triple
        {
            Point previous;
            Point target;
            Point next;
            Color color;

            public Triple(Point p, Point c, Point n, Color cl)
            {
                previous = p; target = c; next = n; color = cl;
            }

            public Vector3 ForwardDirection {
                get
                {
                    return Curve.AutoDirection(target, next, 0f);
                }
            }
            public Vector3 BackDirection
            {
                get
                {
                    return Curve.AutoDirection(previous, target, 1f);
                }
            }
            public Vector3 Position
            {
                get
                {
                    return target.position;
                }
            }
            public float CurrentWidth
            {
                get
                {
                    return target.width;
                }
            }
            public Color CurrentColor
            {
                get
                {
                    return color;
                }
            }
        }

        public enum LineMode
        {
            SplineMode,
            BezierMode
        }
        [SerializeField]
        LineMode mode;
        [SerializeField]
        LinePair pair;
        [SerializeField]
        List<Point> points;
        
        public LineOption option;
                
        public static Spline Default
        {
            get
            {
                return new Spline
                {
                    points = new List<Point>() { Point.Zero, new Point(Vector3.right * 10, Vector3.zero, Vector3.zero) },
                    mode = LineMode.SplineMode,
                    pair = new LinePair(Point.Zero, new Point(Vector3.right, Vector3.zero, Vector3.zero), 0, 1, 0, 1),
                    option = LineOption.Default
                };
            }
        }

        Point GetFirstPoint()
        {
            if (mode == LineMode.BezierMode)
                return pair.n0;

            if (points.Count < 1)
                throw new Exception("need more point");

            return points[0];
        }
        Point GetLastPoint()
        {
            if (mode == LineMode.BezierMode)
                return pair.n1;

            if (points.Count < 1)
                throw new Exception("need more point");

            return points[points.Count - 1];
        }

        int GetCount()
        {
            if (mode == LineMode.BezierMode)
                return 2;
            
            return points.Count;
        }

        public Vector3 GetPosition(float ratio)
        {
            ratio = Mathf.Clamp01(ratio);

            var cl = ratio * Length;

            foreach (var pair in TargetPairList)
            {
                if (cl > pair.Length)
                    cl -= pair.Length;
                else
                    return pair.GetPoisition(cl / pair.Length);
            }

            return option.mode == LineOption.Mode.Loop ? GetFirstPoint().position : GetLastPoint().position;
        }

        public Vector3 GetDirection(float ratio)
        {
            ratio = Mathf.Clamp01(ratio);
            var cl = ratio * Length;
            Vector3 dir = Vector3.zero;
            foreach (var pair in TargetPairList)
            {
                dir = pair.GetDirection(cl / pair.Length);
                if (cl > pair.Length)
                    cl -= pair.Length;
                else
                    break;
            }
            return dir;
        }

        public IEnumerator<Point> GetEnumerator()
        {
            if(mode== LineMode.BezierMode)
            {
                yield return pair.n0;
                yield return pair.n1;
            }
            else
            {
                foreach (var p in points)
                    yield return p;
            }
        }

        public IEnumerable<Point> TripleEnumerator()
        {
            if (mode == LineMode.BezierMode)
            {
                yield return pair.n0;
                yield return pair.n1;
            }
            else
            {
                foreach (var p in points)
                    yield return p;

                if (option.mode == LineOption.Mode.Loop)
                {
                    yield return points[0];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public float AllLength {
            get
            {
                var length = 0f;
                foreach (var pair in AllPair)
                    length += CurveLength.Auto(pair[0], pair[1]);

                return length;
            }
        }
        public float Length
        {
            get
            {
                var length = 0f;
                foreach (var pair in TargetPairList)
                    length = pair.Length;

                return length;
            }
        }

        IEnumerable<Point[]> AllPair
        {
            get
            {
                bool first = true;
                Point firstPoint = Point.Zero;
                Point lastPoint = Point.Zero;
                Point prev = Point.Zero;
                int count = 0;

                foreach(var p in this)
                {
                    count++;
                    if (first)
                    {
                        firstPoint = p;
                        lastPoint = p;
                        first = false;
                        continue;
                    }

                    yield return new Point[] {lastPoint, p};

                    lastPoint = p;
                }
                              
                if (option.mode == LineOption.Mode.Loop && count > 1)
                {
                    yield return new Point[] { lastPoint, firstPoint };
                }
            }
        }

        public IEnumerable<LinePair> TargetPairList
        {
            get
            {
                var l = AllLength;
                var ls = l * option.startRatio;
                var le = l * option.endRatio;
                var ps = 0f;
                var pe = 0f;
                var pl = 0f;

                if (ls >= le)
                    yield break;

                foreach (var pair in AllPair)
                {
                    pl = CurveLength.Auto(pair[0], pair[1]);
                    pe = ps + pl;

                    if (le < ps)
                        yield break;
                    if (ls < pe)
                        yield return new LinePair(pair[0], pair[1], Mathf.Max(0f, (ls - ps) / pl), Mathf.Min(1f, (le - ps) / pl), ps / l, pe / l);
                    ps = pe;
                }
            }
        }
        [Serializable]
        public struct LinePair
        {
            public Point n0;
            public Point n1;
            [NonSerialized]
            public float sRatio;
            [NonSerialized]
            public float eRatio;
            public float RatioLength
            {
                get
                {
                    return eRatio - sRatio;
                }
            }
            [NonSerialized]
            public float start;
            [NonSerialized]
            public float end;
            public LinePair(Point n0, Point n1, float s, float e, float sr, float er)
            {
                this.n0 = n0;
                this.n1 = n1;
                start = s;
                end = e;
                sRatio = sr;
                eRatio = er;
            }
            public float Length
            {
                get
                {
                    return CurveLength.Auto(n0, n1) * (end - start);
                }
            }

            public float GetDT(float divideLength)
            {
                return (divideLength / Length) * (end - start);
            }
            public Vector3 GetPoisition(float r)
            {
                return Curve.Auto(n0, n1, Mathf.Lerp(start, end, r));
            }
            public Vector3 GetDirection(float r)
            {
                return Curve.AutoDirection(n0, n1, Mathf.Lerp(start, end, r));
            }
            public float GetWidth(float t)
            {
                return Mathf.Lerp(n0.width, n1.width, Mathf.Lerp(start, end, t));
            }
        }
    }
}