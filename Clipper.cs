using System;
using System.Collections.Generic;
using System.Drawing;

namespace PKG_5
{
    struct Vec
    {
        public Vec(KeyValuePair<PointF, PointF> a)
        {
            A = a.Key.X - a.Value.X;
            B = a.Key.Y - a.Value.Y;
        }
        public Vec(float Vx, float Vy)
        {
            this.A = Vx;
            this.B = Vy;
        }
        public Vec(PointF begin, PointF end)
        {
            A = begin.X - end.X;
            B = begin.Y - end.Y;
        }
        public float A;
        public float B;
    }
    struct RectangleClipper
    {
        public RectangleClipper(Point min, Point max)
        {
            pMin = min;
            pMax = max;
        }
        public RectangleClipper(float x_min, float y_min, float x_max, float y_max)
        {
            pMin = new PointF(x_min, y_min);
            pMax = new PointF(x_max, y_max);
        }
        public PointF pMin;
        public PointF pMax;
    }
    class Clipper
    {
        public void SetRectangleClipper(float x_min, float y_min, float x_max, float y_max)
        {
            rect = new RectangleClipper(x_min, y_min, x_max, y_max);
        }
        public void SetRectangleClipper(Point min, Point max)
        {
            rect = new RectangleClipper(min, max);
        }
        public bool LiangBarski(PointF p1, PointF p2, ref float t_enter, ref float t_outer)
        {
            double[] Q = new double[4];
            double[] S = new double[4];
            var x1 = p1.X;
            var y1 = p1.Y;
            var x2 = p2.X;
            var y2 = p2.Y;
            var dx = x2 - x1;
            var dy = y2 - y1;
            Q[0] = y1 - rect.pMin.Y;
            Q[1] = x1 - rect.pMin.X;
            Q[2] = rect.pMax.Y - y1;
            Q[3] = rect.pMax.X - x1;
            S[0] = -dy;
            S[1] = -dx;
            S[2] = dy;
            S[3] = dx;
            t_enter = 0;
            t_outer = 1;
            for (int i = 0; i < 4; i++)
            {
                if (S[i] > 0)
                {
                    t_outer = (float)Math.Min(Q[i] / S[i], t_outer);
                }
                else if (S[i] < 0)
                {
                    t_enter = (float)Math.Max(Q[i] / S[i], t_enter);
                }
                else
                {
                    continue;
                }
            }
            if (t_enter > t_outer)
            {
                t_enter = -1;
                t_outer = -1;
            }
            return true;
        }
        public RectangleClipper rect;
        public List<Vec> vectors;
        public List<KeyValuePair<PointF, PointF>> edges;
        public float ScalarMultiply(Vec vec1, Vec vec2)
        {
            return (vec1.A * vec2.A + vec1.B * vec2.B);
        }
        public float VectorMultiply(Vec vec1, Vec vec2)
        {
            return vec1.A * vec2.B - vec1.B * vec2.A;
        }
        public void SetPolygon(List<KeyValuePair<PointF, PointF>> list)
        {
            vectors = new List<Vec>();
            edges = new List<KeyValuePair<PointF, PointF>>(list);
            for (int i = 0; i < list.Count; i++)
            {
                vectors.Add(new Vec(list[i].Key, list[i].Value));
            }

        }

        public bool isConvex(int modify)
        {
            void changeDirection(int direction)
            {
                if (direction > 0)
                {
                    direction = 1;
                }
                else if (direction < 0)
                {
                    direction = -1;
                }
                else
                {
                    return;
                }

                for (int i = 0; i < vectors.Count - 1; i++)
                {
                    edges[i] = new KeyValuePair<PointF, PointF>(edges[i].Value, edges[i].Key);
                    vectors[i] = new Vec(direction * vectors[i].A, direction * vectors[i].B);
                }
            }

            int positive_counter = 0;
            int negative_counter = 0;
            float result;
            for (int i = 0; i < vectors.Count - 1; i++)
            {
                result = ScalarMultiply(vectors[i], vectors[i + 1]);
                if (result > 0)
                {
                    positive_counter++;
                }
                else if (result < 0)
                {
                    negative_counter--;
                }
                else
                {
                    positive_counter++;
                    negative_counter++;
                }
            }

            result = ScalarMultiply(vectors[vectors.Count - 1], vectors[0]);

            if (result > 0)
            {
                positive_counter++;
            }
            else if (result < 0)
            {
                negative_counter++;
            }
            else
            {
                positive_counter++;
                negative_counter++;
            }

            if (positive_counter == vectors.Count || negative_counter == vectors.Count)
            {
                changeDirection(modify);
                return true;
            }
            else
            {
                return false;
            }
        }
        public float getParameterOfPoint(PointF p, KeyValuePair<PointF, PointF> segment)
        {
            return (p.X - segment.Key.X) / (segment.Value.X - segment.Key.X);
        }
        public float getT(KeyValuePair<PointF, PointF> edge, KeyValuePair<PointF, PointF> segment, ref bool onSameLine)
        {
            var x0e = edge.Key.X;
            var y0e = edge.Key.Y;
            var x1e = edge.Value.X;
            var y1e = edge.Value.Y;


            var x0s = segment.Key.X;
            var y0s = segment.Key.Y;
            var x1s = segment.Value.X;
            var y1s = segment.Value.Y;

            float ks = (y1s - y0s) / (x1s - x0s);
            float ke = (y1e - y0e) / (x1e - x0e);

            float bs = y0s - ks * x0s;
            float be = y0e - ke * x0e;

            var x = (be - bs) / (ks - ke);
            if ((x - x0e) / (x1e - x0e) <= 0 || (x - x0e) / (x1e - x0e) >= 1)
            {
                return -1;
            }
            var te = (x - x0s) / (x1s - x0s);
            if (float.IsNaN(te) && ke == ks && be == bs)
            {
                onSameLine = true;
            }
            return te;
        }
        public void CyrusBeck(PointF a, PointF b, ref float t_1, ref float t_2)
        {
            if (!isConvex(0))
            {
                return;
            }
            List<float> T_enter = new List<float>();
            List<float> T_outer = new List<float>();
            KeyValuePair<PointF, PointF> segment = new KeyValuePair<PointF, PointF>(a, b);
            var vec_segment = new Vec(segment);
            float t;
            float S;
            bool onSameLine = false;
            for (int i = 0; i < edges.Count; i++)
            {
                t = getT(edges[i], segment, ref onSameLine);
                if (onSameLine)
                {
                    T_enter.Add(getParameterOfPoint(edges[i].Key, segment));
                    T_outer.Add(getParameterOfPoint(edges[i].Value, segment));
                    T_outer.Add(getParameterOfPoint(edges[i].Key, segment));
                    T_enter.Add(getParameterOfPoint(edges[i].Value, segment));
                    onSameLine = false;
                    continue;
                }
                S = -ScalarMultiply(new Vec(-vectors[i].B, vectors[i].A), vec_segment);
                if (t >= 0 && t <= 1)
                {
                    if (S > 0)
                    {
                        T_enter.Add(t);
                    }
                    else if (S < 0)
                    {
                        T_outer.Add(t);
                    }
                    else
                    {
                        T_enter.Add(t);
                        T_outer.Add(t);
                    }
                }
            }
            if (T_outer.Count == 0 && T_enter.Count == 0)
            {
                t_1 = -1;
                t_2 = -1;
                return;
            }
            float t_enter = 0;
            float t_outer = 1;
            for (int i = 0; i < T_enter.Count; i++)
            {
                if (t_enter < T_enter[i])
                {
                    t_enter = T_enter[i];
                }
            }

            for (int i = 0; i < T_outer.Count; i++)
            {
                if (t_outer > T_outer[i])
                {
                    t_outer = T_outer[i];
                }
            }
            t_1 = t_enter;
            t_2 = t_outer;
        }
    }
}