using System;
using System.IO;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Size = System.Drawing.Size;

namespace OpenTemple.Tests.TestUtils
{
    /// <summary>
    /// Used to encode a stream of video frames to an mp4 file encoded in H264. Intended to save
    /// videos of a test case run for easier debugging.
    /// This is based heavily on the example found at https://github.com/Ruslan-B/FFmpeg.AutoGen/blob/master/FFmpeg.AutoGen.Example/H264VideoStreamEncoder.cs
    /// </summary>
    public sealed unsafe class H264Encoder : IDisposable
    {
        private readonly Size _frameSize;
        private readonly int _linesizeU;
        private readonly int _linesizeV;
        private readonly int _linesizeY;
        private AVCodec* _pCodec;
        private AVCodecContext* _pCodecContext;
        private AVFormatContext* _outputContext;
        private AVStream* _stream;
        private AVFrame* _frame;
        private SwsContext* _swsContext;
        private readonly int _uSize;
        private readonly int _ySize;

        public H264Encoder(string outputPath, Size frameSize)
        {
            ffmpeg.RootPath = Path.Join(TestData.SolutionDir, "ffmpeg", "bin");

            _frameSize = frameSize;

            var codecId = AVCodecID.AV_CODEC_ID_H264;
            _pCodec = ffmpeg.avcodec_find_encoder(codecId);
            if (_pCodec == null)
            {
                throw new InvalidOperationException("Codec not found.");
            }

            _pCodecContext = ffmpeg.avcodec_alloc_context3(_pCodec);
            _pCodecContext->width = frameSize.Width;
            _pCodecContext->height = frameSize.Height;
            _pCodecContext->time_base = new AVRational { num = 1, den = 1000 };
            _pCodecContext->pix_fmt = AVPixelFormat.AV_PIX_FMT_YUV420P;
            ffmpeg.av_opt_set(_pCodecContext->priv_data, "preset", "superfast", 0);

            ffmpeg.avcodec_open2(_pCodecContext, _pCodec, null).ThrowExceptionIfError();

            _linesizeY = frameSize.Width;
            _linesizeU = frameSize.Width / 2;
            _linesizeV = frameSize.Width / 2;

            _ySize = _linesizeY * frameSize.Height;
            _uSize = _linesizeU * frameSize.Height / 2;

            _swsContext = null;

            // Allocate a frame
            _frame = ffmpeg.av_frame_alloc();
            _frame->width = _pCodecContext->width;
            _frame->height = _pCodecContext->height;
            _frame->format = (int)_pCodecContext->pix_fmt;
            ffmpeg.av_frame_get_buffer(_frame, 32);

            // Create output context for mp4
            AVFormatContext *outputContext;
            ffmpeg.avformat_alloc_output_context2(&outputContext, null, "mp4", null).ThrowExceptionIfError();
            _outputContext = outputContext;
            ffmpeg.avio_open2(&_outputContext->pb, outputPath, ffmpeg.AVIO_FLAG_WRITE, null, null);

            // Create video stream in mp4 container
            _stream = ffmpeg.avformat_new_stream(_outputContext, _pCodec);
            ffmpeg.avcodec_parameters_from_context(_stream->codecpar, _pCodecContext)
                .ThrowExceptionIfError();
            _stream->sample_aspect_ratio = _pCodecContext->sample_aspect_ratio;
            _stream->time_base = _pCodecContext->time_base;
            ffmpeg.avformat_write_header(_outputContext, null);
        }

        public void Dispose()
        {
            if (_outputContext != null)
            {
                ffmpeg.av_write_trailer(_outputContext);
                ffmpeg.avio_close(_outputContext->pb);
                ffmpeg.avformat_free_context(_outputContext);

                _outputContext = null;
                _stream = null;
            }

            if (_frame != null)
            {
                var frame = _frame;
                ffmpeg.av_frame_free(&frame);
                _frame = frame;
            }

            if (_pCodecContext != null)
            {
                ffmpeg.avcodec_close(_pCodecContext);
                ffmpeg.av_free(_pCodecContext);
                _pCodecContext = null;
            }

            if (_pCodec != null)
            {
                ffmpeg.av_free(_pCodec);
                _pCodec = null;
            }

            if (_swsContext != null)
            {
                ffmpeg.sws_freeContext(_swsContext);
                _swsContext = null;
            }
        }

        private void Encode(AVFrame *frame)
        {
            if (frame->format != (int)_pCodecContext->pix_fmt)
                throw new ArgumentException("Invalid pixel format.", nameof(frame));
            if (frame->width != _frameSize.Width) throw new ArgumentException("Invalid width.", nameof(frame));
            if (frame->height != _frameSize.Height) throw new ArgumentException("Invalid height.", nameof(frame));
            if (frame->linesize[0] < _linesizeY) throw new ArgumentException("Invalid Y linesize.", nameof(frame));
            if (frame->linesize[1] < _linesizeU) throw new ArgumentException("Invalid U linesize.", nameof(frame));
            if (frame->linesize[2] < _linesizeV) throw new ArgumentException("Invalid V linesize.", nameof(frame));
            if (frame->data[1] - frame->data[0] < _ySize)
                throw new ArgumentException("Invalid Y data size.", nameof(frame));
            if (frame->data[2] - frame->data[1] < _uSize)
                throw new ArgumentException("Invalid U data size.", nameof(frame));

            var pPacket = ffmpeg.av_packet_alloc();

            try
            {
                int error;

                do
                {
                    ffmpeg.avcodec_send_frame(_pCodecContext, frame).ThrowExceptionIfError();
                    ffmpeg.av_packet_unref(pPacket);
                    error = ffmpeg.avcodec_receive_packet(_pCodecContext, pPacket);
                } while (error == ffmpeg.AVERROR(ffmpeg.EAGAIN));

                error.ThrowExceptionIfError();

                /* prepare packet for muxing */
                pPacket->stream_index = _stream->index;
                ffmpeg.av_packet_rescale_ts(pPacket,
                    _pCodecContext->time_base,
                    _stream->time_base);

                ffmpeg.av_interleaved_write_frame(_outputContext, pPacket).ThrowExceptionIfError();
            }
            finally
            {
                ffmpeg.av_packet_free(&pPacket);
            }
        }

        public void Encode(Image<Bgra32> image, long ptsMilliseconds)
        {
            _swsContext = ffmpeg.sws_getCachedContext(_swsContext,
                image.Width, image.Height, AVPixelFormat.AV_PIX_FMT_BGRA,
                _pCodecContext->width, _pCodecContext->height, _pCodecContext->pix_fmt,
                ffmpeg.SWS_POINT, null, null, null);

            if (_swsContext == null)
            {
                throw new InvalidOperationException("Failed to create or update the SWS context.");
            }

            if (!image.TryGetSinglePixelSpan(out var pixelData))
            {
                throw new InvalidOperationException("Cannot get raw pixel data of frame");
            }

            fixed (Bgra32* ptr = pixelData)
            {
                var data = new byte*[4] { (byte*) ptr, null, null, null };
                var linesize = new int[4] { 4 * image.Width, 0, 0, 0 };
                ffmpeg.sws_scale(_swsContext, data, linesize, 0, image.Height, _frame->data, _frame->linesize)
                    .ThrowExceptionIfError();
            }

            // Set presentation time
            _frame->pts = ptsMilliseconds;

            Encode(_frame);
        }
    }

    // Source: https://github.com/Ruslan-B/FFmpeg.AutoGen/blob/master/FFmpeg.AutoGen.Example/FFmpegHelper.cs
    internal static class FFmpegHelper
    {
        public static unsafe string av_strerror(int error)
        {
            var bufferSize = 1024;
            var buffer = stackalloc byte[bufferSize];
            ffmpeg.av_strerror(error, buffer, (ulong)bufferSize);
            var message = Marshal.PtrToStringAnsi((IntPtr)buffer);
            return message;
        }

        public static int ThrowExceptionIfError(this int error)
        {
            if (error < 0) throw new ApplicationException(av_strerror(error));
            return error;
        }
    }
}