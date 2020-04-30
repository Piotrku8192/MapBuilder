using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;

namespace MapBuilder {
    class Program {
        static Bitmap bitmap;
        static Dictionary<string, Province> provinces = new Dictionary<string, Province>();
        static List<Border> borders = new List<Border>();
        static string path;

        static void Main(string[] args) {
            Console.WriteLine("EuropeanWars 2020 MapBuilder by Piotr Szulakowski");
            Console.WriteLine("Provinces texture path: ");
            bitmap = ScaleBitmap(new Bitmap(Console.ReadLine()));
            //bitmap = ResizeImage(Image.FromFile(Console.ReadLine()), 4096, 2048);

            Console.WriteLine("Destination path: ");
            path = Console.ReadLine();
            bitmap.Save(path + "\\resized.bmp");

            Console.WriteLine("Generating meshes...");
            provinces = GetProvinces();

            Console.WriteLine("Generating borders...");
            borders = GetBorders();

            Console.WriteLine("Saveing...");
            string json = JsonConvert.SerializeObject(provinces, Formatting.Indented);
            File.WriteAllText(path + "\\provinces.json", json);
            string b = JsonConvert.SerializeObject(borders, Formatting.Indented);
            File.WriteAllText(path + "\\borders.json", b);
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height) {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage)) {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                graphics.SmoothingMode = SmoothingMode.HighSpeed;
                graphics.PixelOffsetMode = PixelOffsetMode.Half;

                using (var wrapMode = new ImageAttributes()) {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        static Bitmap ScaleBitmap(Bitmap input) {
            Bitmap result = new Bitmap(input.Width * 2, input.Height * 2);
            Color[][] pixels = new Color[input.Width][];
            for (int x = 0; x < input.Width; x++) {
                pixels[x] = new Color[input.Height];
                for (int y = 0; y < input.Height; y++) {
                    pixels[x][y] = input.GetPixel(x, y);
                }
            }

            for (int x = 0; x < input.Width; x++) {
                for (int y = 0; y < input.Height; y++) {
                    result.SetPixel(x * 2, y * 2, pixels[x][y]);
                    result.SetPixel((x * 2) + 1, y * 2, pixels[x][y]);
                    result.SetPixel(x * 2, (y * 2) + 1, pixels[x][y]);
                    result.SetPixel((x * 2) + 1, (y * 2) + 1, pixels[x][y]);

                    if (x > 0 && y < input.Height - 1) {
                        if (pixels[x - 1][y + 1] == pixels[x - 1][y]
                            && pixels[x - 1][y + 1] == pixels[x][y + 1]) {
                            result.SetPixel(x * 2, (y * 2) + 1, pixels[x - 1][y]);
                        }
                    }

                    if (x < input.Width - 1 && y < input.Height - 1) {
                        if (pixels[x + 1][y + 1] == pixels[x + 1][y]
                            && pixels[x + 1][y + 1] == pixels[x][y + 1]) {
                            result.SetPixel((x * 2) + 1, (y * 2) + 1, pixels[x + 1][y]);
                        }
                    }
                }
            }

            return result;
        }

        static Dictionary<string, Province> GetProvinces() {
            Dictionary<string, Province> result = new Dictionary<string, Province>();

            List<Dictionary<string, List<Vector2>>> columnPoses = new List<Dictionary<string, List<Vector2>>>();
            for (int x = 0; x < bitmap.Width; x++) {
                columnPoses.Add(new Dictionary<string, List<Vector2>>());
                for (int y = 0; y < bitmap.Height; y++) {
                    Color pixel = bitmap.GetPixel(x, y);
                    string spixel = pixel.R.ToString("X2") + pixel.G.ToString("X2") + pixel.B.ToString("X2");

                    if (!result.ContainsKey(spixel)) {
                        result.Add(spixel, new Province());
                    }
                    if (!columnPoses[x].ContainsKey(spixel)) {
                        columnPoses[x].Add(spixel, new List<Vector2>());
                    }

                    if (y == 0 || y == bitmap.Height - 1
                        || x == 0 || x == bitmap.Width - 1
                        || bitmap.GetPixel(x, y - 1) != pixel
                        || bitmap.GetPixel(x, y + 1) != pixel) {
                        columnPoses[x][spixel].Add(new Vector2(x, y));
                        if (y != 0 && y != bitmap.Height - 1
                            && bitmap.GetPixel(x, y - 1) != pixel && bitmap.GetPixel(x, y + 1) != pixel) {
                            columnPoses[x][spixel].Add(new Vector2(x, y));
                        }
                    }
                }
            }
            for (int x = 0; x < bitmap.Width; x++) {
                foreach (var item in columnPoses[x]) {
                    for (int i = 0; i < item.Value.Count - 1; i += 2) {
                        Mesh m = result[item.Key].mesh;

                        Vector2 v = item.Value[i];
                        Vector2 vl = v + new Vector2(-0.5f, -0.5f);
                        Vector2 vr = v + new Vector2(0.5f, -0.5f);

                        Vector2 v2 = item.Value[i + 1];
                        Vector2 v2l = v2 + new Vector2(-0.5f, 0.5f);
                        Vector2 v2r = v2 + new Vector2(0.5f, 0.5f);

                        if (m.vertices.Count > 3) {
                            Vector2 _vr = m.vertices[m.vertices.Count - 3];
                            Vector2 _v2r = m.vertices[m.vertices.Count - 1];

                            if (vr.Y == _vr.Y && v2r.Y == _v2r.Y) {
                                m.vertices[m.vertices.Count - 3] = vr;
                                m.vertices[m.vertices.Count - 1] = v2r;
                                continue;
                            }
                        }

                        m.vertices.Add(vl);
                        m.vertices.Add(vr);

                        m.vertices.Add(v2l);
                        m.vertices.Add(v2r);

                        int c = m.vertices.Count;
                        int[] indices = new int[6] { c - 4, c - 1, c - 2, c - 3, c - 1, c - 4 };
                        m.indices.AddRange(indices);
                    }
                }
            } 
            return result;
        }

        static List<Border> GetBorders() {
            Dictionary<string, List<BorderPoint>> points = new Dictionary<string, List<BorderPoint>>();

            for (int x = 1; x < bitmap.Width - 1; x++) {
                for (int y = 1; y < bitmap.Height - 1; y++) {
                    Color pixel = bitmap.GetPixel(x, y);
                    string spixel = pixel.R.ToString("X2") + pixel.G.ToString("X2") + pixel.B.ToString("X2");

                    BorderPoint point = BorderPoint.GetPoint(x, y, bitmap);
                    if (point.position != new Vector2(-1, -1)) {
                        if (!points.ContainsKey(spixel)) {
                            points.Add(spixel, new List<BorderPoint>());
                        }
                        points[spixel].Add(point);
                    }
                }
            }

            List<Border> result = new List<Border>();
            int i = 0;
            foreach (var item in points) {
                i++;
                Console.WriteLine($"Generating borders... ({i}/{points.Count()})");

                while (item.Value.Count > 0) {
                    BorderPoint start = item.Value[0];
                    BorderPoint last = start;

                    item.Value.Remove(start);
                    BorderPoint current = GetNextPoint(last, item.Value);
                    item.Value.Remove(current);
                    Border lastB = null;
                    Border b = null;

                    if (current.position == new Vector2(-1, -1)) {
                        continue;
                    }

                    do {
                        b = result.Where(t => t.firstProvince == item.Key && t.secondProvince == current.borderProvince).FirstOrDefault();
                        if (b == null) {
                            var n = result.Where(t => t.firstProvince == current.borderProvince && t.secondProvince == item.Key);
                            if (n.Any()) {
                                b = n.FirstOrDefault();
                                lastB?.vertices?.Last().Add(current.fixedPosition);
                                goto Skip;
                            }
                            else {
                                Border border = new Border() {
                                    firstProvince = item.Key,
                                    secondProvince = current.borderProvince,
                                    vertices = new List<List<Vector2>>()
                                };
                                border.vertices.Add(new List<Vector2>());
                                border.vertices.Last().Add(last.fixedPosition);

                                result.Add(border);
                                b = border;
                            }
                        }
                        lastB = b;

                        if (Vector2.Distance(b.vertices.Last().Last(), current.fixedPosition) >= 1.7f) {
                            b.vertices.Add(new List<Vector2>());
                            b.vertices.Last().Add(last.fixedPosition);
                        }

                        b.vertices.Last().Add(current.fixedPosition);
                        last = current;

                    Skip:
                        current = GetNextPoint(last, item.Value);
                        item.Value.Remove(current);

                        

                    } while (current.position != new Vector2(-1, -1) && current.position != start.position);
                }
            }

            Console.WriteLine("Optimalizing borders...");
            result = OptimalizeBorders(result);

            return result;
        }

        static BorderPoint GetNextPoint(BorderPoint last, List<BorderPoint> points) {
            Vector2[] poses = new Vector2[8] {
                new Vector2(0, -1),
                new Vector2(1, -1),
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1),
                new Vector2(-1, 1),
                new Vector2(-1, 0),
                new Vector2(-1, -1),
            };

            for (int i = 0; i < poses.Length; i++) {
                var pos = points.Where(t => t.position == last.position + poses[i]);
                if (pos.Any()) {
                    return pos.FirstOrDefault();
                }
            }

            return new BorderPoint() { position = new Vector2(-1, -1) };
        }

        static List<Border> OptimalizeBorders(List<Border> b) {
            foreach (var item in b) {
                foreach (var v in item.vertices) {
                    List<Vector2> toRemove = new List<Vector2>();
                    for (int i = 0; i < v.Count; i++) {
                        if (i > 1) {
                            if (v[i].X == v[i - 1].X) {
                                if (v[i].X == v[i - 2].X) {
                                    toRemove.Add(v[i - 1]);
                                }
                            }
                            else if (v[i].Y == v[i - 1].Y) {
                                if (v[i].Y == v[i - 2].Y) {
                                    toRemove.Add(v[i - 1]);
                                }
                            }
                        }
                    }

                    foreach (var t in toRemove) {
                        v.Remove(t);
                    }
                }
            }

            return b;
        }
    }
}
