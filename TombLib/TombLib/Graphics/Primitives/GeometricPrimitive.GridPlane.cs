// Copyright (c) 2010-2014 SharpDX - Alexandre Mutel
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Numerics;
using System.Collections.Generic;
using TombLib.Graphics.Dx11Toolkit;

namespace TombLib.Graphics.Primitives
{
    using Vector3 = System.Numerics.Vector3;
    using Vector4 = System.Numerics.Vector4;

    public partial class GeometricPrimitive
    {
        /// <summary>
        /// A plane primitive.
        /// </summary>
        public struct GridPlane
        {
            /// <summary>
            /// Creates a Plane primitive on the X/Y plane with a normal equal to -<see cref="Vector3.UnitZ" />.
            /// </summary>
            /// <param name="device">The device.</param>
            /// <param name="sizeX">The size X.</param>
            /// <param name="sizeY">The size Y.</param>
            /// <param name="tessellation">The tessellation, as the number of quads per axis.</param>
            /// <param name="uvFactor">The uv factor.</param>
            /// <param name="toLeftHanded">if set to <c>true</c> vertices and indices will be transformed to left handed. Default is true.</param>
            /// <returns>A Plane primitive.</returns>
            /// <exception cref="System.ArgumentOutOfRangeException">tessellation;tessellation must be &gt; 0</exception>
            public static GeometricPrimitive New(GraphicsDevice device, float size, int tessellation, bool toLeftHanded = false)
            {
                if (tessellation < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(tessellation), "tessellation must be > 0");
                }

                int start = (int)(-size * 1024.0f);
                int end = (int)(size * 1024.0f);
                int step = (int)(1024.0f / tessellation);

                var vertices = new List<SolidVertex>();
                var indices = new List<int>();

                int lastIndex = 0;

                for (int x = start; x <= end; x += step)
                {
                    var color = Vector4.One;
                    if (x % 1024 != 0) color = new Vector4(0.75f, 0.75f, 0.75f, 1.0f);
                    if (x == 0) color = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);

                    var v1 = new SolidVertex();
                    v1.Position = new Vector3(x, 0.0f, x == 0 ? -32768.0f : start);
                    v1.Color = color;

                    var v2 = new SolidVertex();
                    v2.Position = new Vector3(x, 0.0f, x == 0 ? 32768.0f : end);
                    v2.Color = color;

                    vertices.Add(v1);
                    vertices.Add(v2);

                    indices.Add(lastIndex++);
                    indices.Add(lastIndex++);
                }

                for (int z = start; z <= end; z += step)
                {
                    var color = Vector4.One;
                    if (z % 1024 != 0) color = new Vector4(0.75f, 0.75f, 0.75f, 1.0f);
                    if (z == 0) color = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);

                    var v1 = new SolidVertex();
                    v1.Position = new Vector3(z == 0 ? -32768.0f : start, 0.0f, z);
                    v1.Color = color;

                    var v2 = new SolidVertex();
                    v2.Position = new Vector3(z == 0 ? 32768.0f : end, 0.0f, z);
                    v2.Color = color;

                    vertices.Add(v1);
                    vertices.Add(v2);

                    indices.Add(lastIndex++);
                    indices.Add(lastIndex++);
                }

                // Add a final Y axis line
                var vy1 = new SolidVertex();
                vy1.Position = new Vector3(0.0f, -32768.0f, 0.0f);
                vy1.Color = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);

                var vy2 = new SolidVertex();
                vy2.Position = new Vector3(0.0f, 32768.0f, 0.0f);
                vy2.Color = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);

                vertices.Add(vy1);
                vertices.Add(vy2);

                indices.Add(lastIndex++);
                indices.Add(lastIndex++);

                // Create the primitive object.
                return new GeometricPrimitive(device, vertices.ToArray(), indices.ToArray(), toLeftHanded) { Name = "Grid"};
            }
        }
   }
}
