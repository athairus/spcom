/*
 * Copyright (c) 2012 Milton John (whiplash)
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files 
 * (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, 
 * publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
 * FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION 
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using System;

namespace spcom {
    partial class it {
        // byte:   1 byte
        // ushort: 2 bytes
        // uint:   4 bytes
        // ulong:  8 bytes

        #region Impulse Tracker header

        /// <summary> 0x00 Magic word. Should be 'IMPM' or 0x4D504D49. </summary>
        uint magicword;

        /// <summary> 0x04 Song name. ASCII, 26 characters max.</summary>
        string songname;

        /// <summary> 0x1E Pattern row highlight information. Only relevant for pattern editing situations.</summary>
        ushort pathighlight;

        /// <summary> 0x20 Number of orders in the song. </summary>
        ushort numorders;

        /// <summary> 0x22 Number of instruments in the song. </summary>
        ushort insnum;

        /// <summary> 0x24 Number of samples in the song. </summary>
        ushort numsamples;

        /// <summary> 0x26 Number of patterns in the song. </summary>
        ushort numpatterns;

        /// <summary> 0x28 Created with tracker. Impulse Tracker y.xx = 0yxxh </summary>
        ushort createdwithtracker;

        /// <summary> 0x2A Compatible with tracker with version greater than value. (ie. format version) </summary>
        ushort compatablewithtracker;

        /// <summary> 0x2C Various flags. See 'impulse tracker specification.txt' or visit http://16-bits.org/it/ for details. </summary>
        ushort flags;

        /// <summary> 0x2E Even more flags. See 'impulse tracker specification.txt' or visit http://16-bits.org/it/ for details. </summary>
        ushort special;

        /// <summary> 0x30 Global volume. (0->128) All volumes are adjusted by this. </summary>
        byte globalvol;

        /// <summary> 0x31 Mix volume (0->128) During mixing, this value controls the magnitude of the wave being mixed. </summary>
        byte mixvol;

        /// <summary> 0x32 Initial speed of the song. </summary>
        byte initialspeed;

        /// <summary> 0x33 Initial tempo of the song. </summary>
        byte initialtempo;

        /// <summary> 0x34 Panning separation between channels (0->128, 128 is max sep.) </summary>
        byte panseperation;

        /// <summary> 0x35 Pitch wheel depth for MIDI controllers. </summary>
        byte pitchwheeldepth;

        /// <summary> 0x36 Length of the song message, in bytes. Note: v1.04+ of IT may have song messages of up to 8000 bytes included. </summary>
        ushort messagelength;

        /// <summary> 0x38 Offset of the song's message. </summary>
        uint messageoffset;

        /// <summary> Song message, newline character = 0x0D, max size: 8000 bytes. </summary>
        byte[] message;

        /// <summary> 0x3C Reserved for future use? </summary>
        uint reserved;

        /// <summary> 0x40 Each byte contains a panning value for a channel. Ranges from 0 (absolute left) to 64 (absolute right). </summary>
        byte[] channelpan;

        /// <summary> 0x80 Volume for each channel. Ranges from 0->64. </summary>
        byte[] channelvol;

        /// <summary> 0xC0 The order in which the patterns are played. </summary>
        byte[] orders;

        /// <summary> Offsets of the samples. </summary>
        uint[] sampleoffsets;

        /// <summary> Offsets of the patterns. </summary>
        uint[] patternoffsets;

        /// <summary> Number of channels, calculated by checking the channel pans (<code>channelpan</code>). This is an internal variable not found in the actual header. </summary>
        byte channels;

        #endregion

        #region Impulse Tracker sample header
        itsample[] samples;
        /// <summary> Impulse Tracker sample header. </summary>
        struct itsample {
            /// <summary> 0x00 Magic word. Should be 'IMPS' or 0x53504D49. </summary>
            public uint magicword;

            /// <summary> 0x04 DOS Filename of the format (12345678.123). </summary>
            public string dosfilename;

            /// <summary> 0x11 Global volume for instrument, ranges from 0->64. </summary>
            public byte globalvol;

            /// <summary> 0x12 Various sample header flags. See 'impulse tracker specification.txt' or visit http://16-bits.org/it/ for details. </summary>
            public byte flags;

            /// <summary> 0x13 Default volume for instrument. </summary>
            public byte defaultvol;

            /// <summary> 0x14 Sample name. ASCII, 26 characters max. </summary>
            public string samplename;

            /// <summary> 0x2E Bits other than bit 0 are used internally for the loading of alternative formats. </summary>
            public byte convert;

            /// <summary> 0x2F Default Pan. Bits 0->6 = Pan value, Bit 7 ON to USE (opposite of inst). </summary>
            public byte defaultpan;

            /// <summary> 0x30 Length of sample in no. of samples, NOT no. of bytes. </summary>
            public uint length;

            /// <summary> 0x34 Start of loop (no of samples in, not bytes). </summary>
            public uint loopbegin;

            /// <summary> 0x38 Sample no. AFTER end of loop. </summary>
            public uint loopend;

            /// <summary> 0x3C Number of bytes a second for C-5 (ranges from 0->9999999). </summary>
            public uint c5speed;

            /// <summary> 0x40 Start of the sustain loop. </summary>
            public uint susloopbegin;

            /// <summary> 0x44 Sample no. AFTER the end of sustain loop. </summary>
            public uint susloopend;

            /// <summary> 0x48 Pointer to the actual sample data. </summary>
            public uint samplepointer;

            /// <summary> 0x4C Vibrato Speed, ranges from 0->64. </summary>
            public byte vibratospeed;

            /// <summary> 0x4D Vibrato Depth, ranges from 0->64. </summary>
            public byte vibratodepth;

            /// <summary> 0x4E Vibrato Rate, rate at which vibrato is applied (0->64). </summary>
            public byte vibratorate;

            /// <summary> 0x4F Vibrato waveform type. </summary>
            public byte vwaveformtype;

            /// <summary> Sample data. </summary>
            public byte[] sample; 
            
        }

        #endregion

        #region Impulse Tracker pattern header
        itpattern[] patterns;
        /// <summary> Impulse Tracker pattern header. </summary>
        struct itpattern {
            /// <summary> 0x00 Length of the packed pattern, not including 8-byte header. </summary>
            public ushort length;

            /// <summary> 0x02 Number of rows in this pattern. </summary>
            public ushort rows;

            /// <summary> 0x08 Packed pattern data. </summary>
            public byte[] packeddata;

            /// <summary> Unpacked pattern data. </summary>
            //public byte[] unpackeddata;

            /// <summary> Unpacked pattern data, stored row by row. </summary>
            public rowgroup[] row;
        }
        struct rowgroup {
            public channelgroup[] channel;
        }

        /// <summary> Various commands stored in a struct form for easy access. </summary>
        struct channelgroup {
            public byte note;
            public byte instrument;
            public byte volume;
            public byte command;
            public byte parameter;
        }
        


        #endregion


        /// <summary></summary>
    }
}