using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Backrole.Orp.Internals
{
    internal static class TcpClientExtensions
    {
        /// <summary>
        /// Receive fulfilled array from the remote host asynchronously.
        /// When the connection closed or impossible to recover, returns null.
        /// </summary>
        /// <exception cref="OperationCanceledException">when the token triggered.</exception>
        /// <param name="Tcp"></param>
        /// <param name="Size"></param>
        /// <param name="Token"></param>
        /// <returns></returns>
        public static Task<byte[]> ReceiveAsync(this TcpClient Tcp, int Size, CancellationToken Token = default)
            => Tcp.ReceiveAsync(Size, false, Token);

        /// <summary>
        /// Receive fulfilled array from the remote host asynchronously.
        /// When the connection closed or impossible to recover, returns null.
        /// </summary>
        /// <exception cref="OperationCanceledException">when the token triggered.</exception>
        /// <param name="Tcp"></param>
        /// <param name="Size"></param>
        /// <param name="NoThrow">suppress any exceptions. in this case, the implementor should check the connection closed or not.</param>
        /// <param name="Token"></param>
        /// <returns></returns>
        public static async Task<byte[]> ReceiveAsync(this TcpClient Tcp, int Size, bool NoThrow, CancellationToken Token = default)
        {
            var Buffer = new ArraySegment<byte>(new byte[Size]);

            if (Tcp.Connected && !NoThrow)
                Token.ThrowIfCancellationRequested();

            while (Tcp.Connected && !Token.IsCancellationRequested)
            {
                if (Buffer.Count <= 0)
                    return Buffer.Array;

                int Length;
                try   { Length = await Tcp.Client.ReceiveAsync(Buffer, SocketFlags.None, Token); }
                catch { Length = 0; }

                if (Length <= 0)
                    continue;

                Buffer = new ArraySegment<byte>(Buffer.Array, Buffer.Offset + Length, Buffer.Count - Length);
            }

            return null;
        }

        /// <summary>
        /// Receive fulfilled array as chunked frame from the remote host asynchronously.
        /// When the connection closed or impossible to recover, returns null.
        /// </summary>
        /// <exception cref="OperationCanceledException">when the token triggered.</exception>
        /// <param name="Tcp"></param>
        /// <param name="Token"></param>
        /// <returns></returns>
        public static async Task<byte[]> ReceiveChunkedAsync(this TcpClient Tcp, CancellationToken Token = default)
        {
            var LB = await Tcp.ReceiveAsync(sizeof(uint), Token);
            if (LB is null)
                return null;

            if (!BitConverter.IsLittleEndian)
                Array.Reverse(LB);

            return await Tcp.ReceiveAsync(BitConverter.ToInt32(LB), true, Token);
        }

        /// <summary>
        /// Emit the packet to the remote host asynchronously.
        /// When the connection closed or impossible to recover, returns false.
        /// </summary>
        /// <exception cref="OperationCanceledException">when the token triggered.</exception>
        /// <param name="Tcp"></param>
        /// <param name="Buffer"></param>
        /// <param name="Token"></param>
        /// <returns></returns>
        public static Task<bool> EmitAsync(this TcpClient Tcp, ArraySegment<byte> Buffer, CancellationToken Token = default)
            => Tcp.EmitAsync(Buffer, false, Token);

        /// <summary>
        /// Emit the packet to the remote host asynchronously.
        /// When the connection closed or impossible to recover, returns false.
        /// </summary>
        /// <exception cref="OperationCanceledException">when the token triggered.</exception>
        /// <param name="Tcp"></param>
        /// <param name="Buffer"></param>
        /// <param name="NoThrow">suppress any exceptions. in this case, the implementor should check the connection closed or not.</param>
        /// <param name="Token"></param>
        /// <returns></returns>
        public static async Task<bool> EmitAsync(this TcpClient Tcp, ArraySegment<byte> Buffer, bool NoThrow, CancellationToken Token = default)
        {
            if (Tcp.Connected && !NoThrow)
                Token.ThrowIfCancellationRequested();

            while (Tcp.Connected && !Token.IsCancellationRequested)
            {
                if (Buffer.Count <= 0)
                    return true;

                int Length;
                try { Length = await Tcp.Client.SendAsync(Buffer, SocketFlags.None, Token); }
                catch { Length = 0; }

                if (Length <= 0)
                    continue;

                Buffer = new ArraySegment<byte>(Buffer.Array, Buffer.Offset + Length, Buffer.Count - Length);
            }

            return false;
        }

        /// <summary>
        /// Emit fulfilled array as chunked frame from the remote host asynchronously.
        /// When the connection closed or impossible to recover, returns false.
        /// </summary>
        /// <exception cref="OperationCanceledException">when the token triggered.</exception>
        /// <param name="Tcp"></param>
        /// <param name="Buffer"></param>
        /// <param name="Token"></param>
        /// <returns></returns>
        public static Task<bool> EmitChunkedAsync(this TcpClient Tcp, ArraySegment<byte> Buffer, CancellationToken Token = default)
        {
            var LB = BitConverter.GetBytes(Buffer.Count);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(LB);

            return Tcp.EmitAsync(LB.Concat(Buffer).ToArray(), Token);
        }
    }
}
