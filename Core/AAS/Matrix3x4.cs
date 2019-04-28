using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace SpicyTemple.Core.AAS {

    [StructLayout(LayoutKind.Sequential)]
    internal struct Matrix3x4
    {
        // Indexing here is col-major
        public float m00;
        public float m01;
        public float m02;
        public float m03;
        public float m10;
        public float m11;
        public float m12;
        public float m13;
        public float m20;
        public float m21;
        public float m22;
        public float m23;

        public Matrix3x4(Matrix4x4 o)
        {
            m00 = o.M11;
            m01 = o.M21;
            m02 = o.M31;
            m03 = o.M41;

            m10 = o.M12;
            m11 = o.M22;
            m12 = o.M32;
            m13 = o.M42;

            m20 = o.M13;
            m21 = o.M23;
            m22 = o.M33;
            m23 = o.M43;
        }

        public const int rows = 4;
        public const int cols = 3;

        public static Matrix3x4 Identity
        {
            get
            {
                var result = new Matrix3x4();
                result.m00 = 1.0f;
                result.m11 = 1.0f;
                result.m22 = 1.0f;
                return result;
            }
        }

        public static implicit operator Matrix4x4(in Matrix3x4 matrix)
        {
            return new Matrix4x4(
                matrix.m00, matrix.m10, matrix.m20, 0,
                matrix.m01, matrix.m11, matrix.m21, 0,
                matrix.m02, matrix.m12, matrix.m22, 0,
                matrix.m03, matrix.m13, matrix.m23, 1.0f
            );
        }

        private Span<float> data => MemoryMarshal.CreateSpan(ref m00, cols * rows);

        public float this[int row, int col]
        {
            get
            {
                Debug.Assert(row >= 0 && row < 4 && col >= 0 && col < 3);
                return data[col * 4 + row];
            }
            set
            {
                Debug.Assert(row >= 0 && row < 4 && col >= 0 && col < 3);
                var elems = data;
                elems[col * 4 + row] = value;
            }
        }

        public bool Equals(Matrix3x4 other)
        {
            return m00.Equals(other.m00) && m01.Equals(other.m01) && m02.Equals(other.m02) && m03.Equals(other.m03) &&
                   m10.Equals(other.m10) && m11.Equals(other.m11) && m12.Equals(other.m12) && m13.Equals(other.m13) &&
                   m20.Equals(other.m20) && m21.Equals(other.m21) && m22.Equals(other.m22) && m23.Equals(other.m23);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is Matrix3x4 other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = m00.GetHashCode();
                hashCode = (hashCode * 397) ^ m01.GetHashCode();
                hashCode = (hashCode * 397) ^ m02.GetHashCode();
                hashCode = (hashCode * 397) ^ m03.GetHashCode();
                hashCode = (hashCode * 397) ^ m10.GetHashCode();
                hashCode = (hashCode * 397) ^ m11.GetHashCode();
                hashCode = (hashCode * 397) ^ m12.GetHashCode();
                hashCode = (hashCode * 397) ^ m13.GetHashCode();
                hashCode = (hashCode * 397) ^ m20.GetHashCode();
                hashCode = (hashCode * 397) ^ m21.GetHashCode();
                hashCode = (hashCode * 397) ^ m22.GetHashCode();
                hashCode = (hashCode * 397) ^ m23.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(Matrix3x4 left, Matrix3x4 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Matrix3x4 left, Matrix3x4 right)
        {
            return !left.Equals(right);
        }

        public static Matrix3x4 scaleMatrix(float x, float y, float z)
        {
            var result = new Matrix3x4();
            result.m00 = x;
            result.m11 = y;
            result.m22 = z;
            return result;
        }

        public static Matrix3x4 translationMatrix(float x, float y, float z)
        {
            var result = Matrix3x4.Identity;
            result.m03 = x;
            result.m13 = y;
            result.m23 = z;
            return result;
        }


        public static Matrix3x4 multiplyMatrix3x3(in Matrix3x4 a, in Matrix3x4 b)
        {
            Matrix3x4 r = new Matrix3x4();
            r.m00 = a.m00 * b.m00 + b.m02 * a.m20 + b.m01 * a.m10;
            r.m01 = a.m21 * b.m02 + b.m00 * a.m01 + a.m11 * b.m01;
            r.m02 = a.m12 * b.m01 + b.m02 * a.m22 + a.m02 * b.m00;
            r.m10 = b.m12 * a.m20 + a.m10 * b.m11 + b.m10 * a.m00;
            r.m11 = a.m11 * b.m11 + b.m10 * a.m01 + b.m12 * a.m21;
            r.m12 = b.m12 * a.m22 + a.m02 * b.m10 + a.m12 * b.m11;
            r.m20 = b.m21 * a.m10 + b.m20 * a.m00 + b.m22 * a.m20;
            r.m21 = b.m21 * a.m11 + b.m20 * a.m01 + b.m22 * a.m21;
            r.m22 = b.m22 * a.m22 + b.m21 * a.m12 + a.m02 * b.m20;
            return r;
        }

        public static Matrix3x4 multiplyMatrix3x3_3x4(in Matrix3x4 a, in Matrix3x4 b)
        {
            Matrix3x4 r = new Matrix3x4();
            r.m00 = a.m00 * b.m00 + b.m02 * a.m20 + b.m01 * a.m10;
            r.m01 = a.m21 * b.m02 + b.m00 * a.m01 + a.m11 * b.m01;
            r.m02 = a.m12 * b.m01 + b.m02 * a.m22 + a.m02 * b.m00;
            r.m03 = b.m03;
            r.m10 = b.m11 * a.m10 + b.m10 * a.m00 + a.m20 * b.m12;
            r.m11 = b.m10 * a.m01 + a.m21 * b.m12 + a.m11 * b.m11;
            r.m12 = a.m12 * b.m11 + a.m02 * b.m10 + a.m22 * b.m12;
            r.m13 = b.m13;
            r.m20 = b.m21 * a.m10 + b.m20 * a.m00 + a.m20 * b.m22;
            r.m21 = b.m20 * a.m01 + a.m21 * b.m22 + a.m11 * b.m21;
            r.m22 = a.m12 * b.m21 + b.m20 * a.m02 + a.m22 * b.m22;
            r.m23 = b.m23;
            return r;
        }

        public static Matrix3x4 multiplyMatrix3x4_3x3(in Matrix3x4 left, in Matrix3x4 right)
        {
            Matrix3x4 r = new Matrix3x4();
            r.m00 = left.m00 * right.m00 + right.m02 * left.m20 + right.m01 * left.m10;
            r.m01 = left.m21 * right.m02 + right.m00 * left.m01 + left.m11 * right.m01;
            r.m02 = left.m12 * right.m01 + right.m02 * left.m22 + left.m02 * right.m00;
            r.m03 = left.m03 * right.m00 + left.m23 * right.m02 + right.m01 * left.m13;
            r.m10 = right.m11 * left.m10 + right.m10 * left.m00 + right.m12 * left.m20;
            r.m11 = right.m11 * left.m11 + right.m10 * left.m01 + right.m12 * left.m21;
            r.m12 = right.m12 * left.m22 + right.m11 * left.m12 + left.m02 * right.m10;
            r.m13 = left.m23 * right.m12 + right.m11 * left.m13 + left.m03 * right.m10;
            r.m20 = right.m22 * left.m20 + right.m21 * left.m10 + right.m20 * left.m00;
            r.m21 = left.m11 * right.m21 + right.m20 * left.m01 + right.m22 * left.m21;
            r.m22 = right.m22 * left.m22 + left.m02 * right.m20 + left.m12 * right.m21;
            r.m23 = right.m20 * left.m03 + left.m23 * right.m22 + left.m13 * right.m21;
            return r;
        }

        public static Matrix3x4 multiplyMatrix3x4(in Matrix3x4 left, in Matrix3x4 right)
        {
            Matrix3x4 result = new Matrix3x4();

            result[0, 0] = left[0, 0] * right[0, 0] + left[0, 1] * right[1, 0] + left[0, 2] * right[2, 0];
            result[1, 0] = left[1, 0] * right[0, 0] + left[1, 1] * right[1, 0] + left[1, 2] * right[2, 0];
            result[2, 0] = left[2, 0] * right[0, 0] + left[2, 1] * right[1, 0] + left[2, 2] * right[2, 0];
            result[3, 0] = left[3, 0] * right[0, 0] + left[3, 1] * right[1, 0] + left[3, 2] * right[2, 0] + right[3, 0];

            result[0, 1] = left[0, 0] * right[0, 1] + left[0, 1] * right[1, 1] + left[0, 2] * right[2, 1];
            result[1, 1] = left[1, 0] * right[0, 1] + left[1, 1] * right[1, 1] + left[1, 2] * right[2, 1];
            result[2, 1] = left[2, 0] * right[0, 1] + left[2, 1] * right[1, 1] + left[2, 2] * right[2, 1];
            result[3, 1] = left[3, 0] * right[0, 1] + left[3, 1] * right[1, 1] + left[3, 2] * right[2, 1] + right[3, 1];

            result[0, 2] = left[0, 0] * right[0, 2] + left[0, 1] * right[1, 2] + left[0, 2] * right[2, 2];
            result[1, 2] = left[1, 0] * right[0, 2] + left[1, 1] * right[1, 2] + left[1, 2] * right[2, 2];
            result[2, 2] = left[2, 0] * right[0, 2] + left[2, 1] * right[1, 2] + left[2, 2] * right[2, 2];
            result[3, 2] = left[3, 0] * right[0, 2] + left[3, 1] * right[1, 2] + left[3, 2] * right[2, 2] + right[3, 2];

            return result;
        }

        public static Matrix3x4 rotationMatrix(Quaternion q)
        {
            var m_dx = Matrix4x4.CreateFromQuaternion(q);
            Matrix3x4 matrix_dx = new Matrix3x4(m_dx);

            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    var value = matrix_dx[row, col];
                    if (MathF.Abs(value) < 0.0001)
                    {
                        value = 0.0f;
                    }
                    else if (MathF.Abs(value - 1.0f) < 0.0001)
                    {
                        value = 1.0f;
                    }
                    else if (MathF.Abs(value + 1.0f) < 0.0001)
                    {
                        value = -1.0f;
                    }
                }
            }

            return matrix_dx;
        }

        public static Vector4 transformPosition(in Matrix3x4 matrix, in Vector4 vector)
        {
            return new Vector4(
                matrix.m00 * vector.X + matrix.m01 * vector.Y + matrix.m02 * vector.Z + matrix.m03,
                matrix.m10 * vector.X + matrix.m11 * vector.Y + matrix.m12 * vector.Z + matrix.m13,
                matrix.m20 * vector.X + matrix.m21 * vector.Y + matrix.m22 * vector.Z + matrix.m23,
                vector.W
            );
        }

        public static Vector4 transformNormal(in Matrix3x4 matrix, in Vector4 vector)
        {
            return new Vector4(
                matrix.m02 * vector.Z + matrix.m01 * vector.Y + matrix.m00 * vector.X,
                matrix.m12 * vector.Z + matrix.m11 * vector.Y + matrix.m10 * vector.X,
                matrix.m22 * vector.Z + matrix.m21 * vector.Y + matrix.m20 * vector.X,
                vector.W
            );
        }


        // This is a fast inverse if the src matrix is an orthogonal affine transformation matrix
        public static Matrix3x4 invertOrthogonalAffineTransform(in Matrix3x4 src) {
            Matrix3x4 result;
            result.m00 = src.m00;
            result.m10 = src.m01;
            result.m20 = src.m02;
            result.m01 = src.m10;
            result.m11 = src.m11;
            result.m21 = src.m12;
            result.m02 = src.m20;
            result.m12 = src.m21;
            result.m22 = src.m22;
            result.m03 = -(src.m23 * src.m20 + src.m13 * src.m10 + src.m03 * src.m00);
            result.m13 = -(src.m23 * src.m21 + src.m13 * src.m11 + src.m03 * src.m01);
            result.m23 = -(src.m23 * src.m22 + src.m13 * src.m12 + src.m03 * src.m02);
            return result;
        }

        public static ref Matrix3x4 makeMatrixOrthogonal(ref Matrix3x4 matrix) {
            for (var i = 0; i < cols; i++) {
                for (var j = 0; j < i; j++)
                {
                    var f = 0.0f;

                    for (int k = 0; k < cols; k++) {
                        f += matrix[k, i] * matrix[k, j];
                    }

                    for (int k = 0; k < cols; k++)
                    {
                        matrix[k, i] -= f * matrix[k, j];
                    }
                }

                // Apparently this normalizes the columns
                var lenSquared = 0.0f;
                for (var j = 0; j < cols; j++) {
                    var v = matrix[j, i];
                    lenSquared += v * v;
                }
                var factor = 1.0f / MathF.Sqrt(lenSquared);

                for (var j = 0; j < cols; j++) {
                    matrix[j, i] *= factor;
                }
            }

            return ref matrix;
        }

    }
}