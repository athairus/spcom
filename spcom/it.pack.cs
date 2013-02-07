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
        /// <summary>
        /// Packs the data contained within the instance of the class this was called from into a .it file, ready to be written to the disk.
        /// </summary>
        /// <returns> A <code>byte</code> array that contains a complete it file, ready to be written to the disk. </returns>
        public byte[] pack() {
            #region Pack the patterns

            ushort rowsize = (ushort) ( channels * 5 );

            for ( int i = 0; i < numpatterns; i++ ) {
                channelgroup[] previousvalues = new channelgroup[ 64 ];
                byte[] channelmask = new byte[ 64 ];
                byte[] prevmaskvar = new byte[ 64 ];

                ushort length = 0;
                patterns[ i ].packeddata = new byte[ 256 * 64 * 7 ];
                for ( int row = 0; row < patterns[ i ].rows; row++ ) {
                    for ( int channel = 0; channel < channels; channel++ ) {
                        byte maskvar = 0;
                        if ( patterns[ i ].row[ row ].channel[ channel ].note != 0 )
                            maskvar |= 1;
                        if ( patterns[ i ].row[ row ].channel[ channel ].instrument != 0 )
                            maskvar |= 2;
                        if ( patterns[ i ].row[ row ].channel[ channel ].volume != 0 )
                            maskvar |= 4;
                        if ( patterns[ i ].row[ row ].channel[ channel ].command != 0 )
                            maskvar |= 8;

                        if ( maskvar != 0 ) {
                            if ( ( maskvar & 1 ) == 1 ) {
                                if ( patterns[ i ].row[ row ].channel[ channel ].note == previousvalues[ channel ].note ) {
                                    unchecked {
                                        maskvar &= (byte) ~0x01;
                                        maskvar |= 0x10;
                                    }
                                }
                                else {
                                    previousvalues[ channel ].note = patterns[ i ].row[ row ].channel[ channel ].note;
                                }
                            }
                            if ( ( maskvar & 2 ) == 2 ) {
                                if ( patterns[ i ].row[ row ].channel[ channel ].instrument == previousvalues[ channel ].instrument ) {
                                    unchecked {
                                        maskvar &= (byte) ~0x02;
                                        maskvar |= 0x20;
                                    }
                                }
                                else {
                                    previousvalues[ channel ].instrument = patterns[ i ].row[ row ].channel[ channel ].instrument;
                                }
                            }
                            if ( ( maskvar & 4 ) == 4 ) {
                                if ( patterns[ i ].row[ row ].channel[ channel ].volume == previousvalues[ channel ].volume ) {
                                    unchecked {
                                        maskvar &= (byte) ~0x04;
                                        maskvar |= 0x40;
                                    }
                                }
                                else {
                                    previousvalues[ channel ].volume = patterns[ i ].row[ row ].channel[ channel ].volume;
                                }
                            }
                            if ( ( maskvar & 8 ) == 8 ) {
                                if ( ( patterns[ i ].row[ row ].channel[ channel ].command == previousvalues[ channel ].command ) && ( patterns[ i ].row[ row ].channel[ channel ].parameter == previousvalues[ channel ].parameter ) ) {
                                    unchecked {
                                        maskvar &= (byte) ~0x08;
                                        maskvar |= 0x80;
                                    }
                                }
                                else {
                                    previousvalues[ channel ].command = patterns[ i ].row[ row ].channel[ channel ].command;
                                    previousvalues[ channel ].parameter = patterns[ i ].row[ row ].channel[ channel ].parameter;
                                }
                            }
                            if ( maskvar != channelmask[ channel ] ) {
                                channelmask[ channel ] = maskvar;
                                patterns[ i ].packeddata[ length++ ] = (byte) ( ( channel + 1 ) | 0x80 );   // write channelvar
                                patterns[ i ].packeddata[ length++ ] = maskvar;                             // write maskvar
                            }
                            else {
                                patterns[ i ].packeddata[ length++ ] = (byte) ( channel + 1 );
                            }
                            if ( ( maskvar & 1 ) == 1 )
                                patterns[ i ].packeddata[ length++ ] = ( patterns[ i ].row[ row ].channel[ channel ].note < 0xFE ) ? (byte) ( patterns[ i ].row[ row ].channel[ channel ].note - 1 ) : patterns[ i ].row[ row ].channel[ channel ].note;
                            if ( ( maskvar & 2 ) == 2 )
                                patterns[ i ].packeddata[ length++ ] = patterns[ i ].row[ row ].channel[ channel ].instrument;
                            if ( ( maskvar & 4 ) == 4 )
                                patterns[ i ].packeddata[ length++ ] = (byte) ( patterns[ i ].row[ row ].channel[ channel ].volume - 1 );
                            if ( ( maskvar & 8 ) == 8 ) {
                                patterns[ i ].packeddata[ length++ ] = patterns[ i ].row[ row ].channel[ channel ].command;
                                patterns[ i ].packeddata[ length++ ] = patterns[ i ].row[ row ].channel[ channel ].parameter;
                            }
                        }

                    }
                    patterns[ i ].packeddata[ length++ ] = 0;
                }
                patterns[ i ].length = length;
                Array.Resize( ref patterns[ i ].packeddata, length );
            }

            #endregion

            #region Calculate file size

            long filesize = 0;
            filesize += 0xC0 + numorders + numsamples * 4 + numpatterns * 4 + 1;
            messageoffset = (uint) ( filesize + 1 );
            filesize += messagelength + 1;
            for ( int i = 0; i < numsamples; i++ ) {
                sampleoffsets[ i ] = (uint) filesize;
                filesize += 0x51;
            }
            for ( int i =0; i < numpatterns; i++ ) {
                patternoffsets[ i ] = (uint) filesize;
                filesize += patterns[ i ].length + 9;
            }
            for ( int i = 0; i < numsamples; i++ ) {
                samples[ i ].samplepointer = (uint) filesize;
                filesize += samples[ i ].length * 2 + 1;
            }

            #endregion

            byte[] data = new byte[ filesize ];

            #region Write the header

            write4( magicword, ref data, 0x00 );

            byte[] songnamearr = System.Text.Encoding.ASCII.GetBytes( songname );
            for ( int i = 0; i < 26; i++ )
                data[ i + 0x04 ] = songnamearr[ i ];

            write2( pathighlight, ref data, 0x1E );

            write2( numorders, ref data, 0x20 );

            write2( insnum, ref data, 0x22 );

            write2( numsamples, ref data, 0x24 );

            write2( numpatterns, ref data, 0x26 );

            write2( createdwithtracker, ref data, 0x28 );

            write2( compatablewithtracker, ref data, 0x2A );

            write2( flags, ref data, 0x2C );

            write2( special, ref data, 0x2E );

            data[ 0x30 ] = globalvol;

            data[ 0x31 ] = mixvol;

            data[ 0x32 ] = initialspeed;

            data[ 0x33 ] = initialtempo;

            data[ 0x34 ] = panseperation;

            data[ 0x35 ] = pitchwheeldepth;

            write2( messagelength, ref data, 0x36 );

            write4( messageoffset, ref data, 0x38 );
            for ( int i = 0; i < messagelength; i++ ) {
                data[ messageoffset + i ] = message[ i ];
            }

            write4( reserved, ref data, 0x3C );

            for ( int i = 0; i < 64; i++ ) {
                data[ 0x40 + i ] = channelpan[ i ];
            }

            for ( int i = 0; i < 64; i++ ) {
                data[ 0x80 + i ] = channelvol[ i ];
            }

            for ( int i = 0; i < numorders; i++ ) {
                data[ 0xC0 + i ] = orders[ i ];
            }

            // ignoring instruments, since they're not supported there shouldn't be any to write

            for ( int i = 0; i < numsamples; i++ ) {
                write4( sampleoffsets[ i ], ref data, 0xC0 + numorders + i * 4 );
            }

            for ( int i = 0; i < numpatterns; i++ ) {
                write4( patternoffsets[ i ], ref data, 0xC0 + numorders + numsamples * 4 + i * 4 );
            }

            #endregion

            #region Write the sample headers

            for ( int i = 0; i < numsamples; i++ ) {
                write4( samples[ i ].magicword, ref data, sampleoffsets[ i ] + 0x00 );

                byte[] dosfilename = System.Text.Encoding.ASCII.GetBytes( samples[ i ].dosfilename );
                for ( int j = 0; j < 12; j++ ) {
                    data[ sampleoffsets[ i ] + 0x04 + j ] = dosfilename[ j ];
                }

                data[ sampleoffsets[ i ] + 0x11 ] = samples[ i ].globalvol;

                data[ sampleoffsets[ i ] + 0x12 ] = samples[ i ].flags;

                data[ sampleoffsets[ i ] + 0x13 ] = samples[ i ].defaultvol;

                byte[] samplename = System.Text.Encoding.ASCII.GetBytes( samples[ i ].samplename );
                for ( int j = 0; j < 26; j++ ) {
                    data[ sampleoffsets[ i ] + 0x14 + j ] = samplename[ j ];
                }

                data[ sampleoffsets[ i ] + 0x2E ] = samples[ i ].convert;

                data[ sampleoffsets[ i ] + 0x2F ] = samples[ i ].defaultpan;

                write4( samples[ i ].length, ref data, sampleoffsets[ i ] + 0x30 );

                write4( samples[ i ].loopbegin, ref data, sampleoffsets[ i ] + 0x34 );

                write4( samples[ i ].loopend, ref data, sampleoffsets[ i ] + 0x38 );

                write4( samples[ i ].c5speed, ref data, sampleoffsets[ i ] + 0x3C );

                write4( samples[ i ].susloopbegin, ref  data, sampleoffsets[ i ] + 0x40 );

                write4( samples[ i ].susloopend, ref data, sampleoffsets[ i ] + 0x44 );

                write4( samples[ i ].samplepointer, ref data, sampleoffsets[ i ] + 0x48 );

                data[ sampleoffsets[ i ] + 0x4C ] = samples[ i ].vibratospeed;

                data[ sampleoffsets[ i ] + 0x4D ] = samples[ i ].vibratodepth;

                data[ sampleoffsets[ i ] + 0x4E ] = samples[ i ].vibratorate;

                data[ sampleoffsets[ i ] + 0x4F ] = samples[ i ].vwaveformtype;

                for ( int j = 0; j < samples[ i ].length * 2; j++ ) {
                    data[ samples[ i ].samplepointer + j ] = samples[ i ].sample[ j ];
                }

            }

            #endregion

            #region Write the pattern headers

            for ( int i = 0; i < numpatterns; i++ ) {

                write2( patterns[ i ].length, ref data, patternoffsets[ i ] + 0x00 );

                write2( patterns[ i ].rows, ref data, patternoffsets[ i ] + 0x02 );

                for ( int j = 0; j < patterns[ i ].length; j++ ) {
                    data[ patternoffsets[ i ] + 0x08 + j ] = patterns[ i ].packeddata[ j ];
                }
            }

            #endregion

            return data;
        }

        private void write2( ushort value, ref byte[] data, long offset ) {
            data[ offset ] = (byte) value;
            data[ offset + 1 ] = (byte) ( value >> 8 );

        }
        private void write4( uint value, ref byte[] data, long offset ) {
            //( ( data[ offset + 3 ] << (byte) 24 ) + ( data[ offset + 2 ] << (byte) 16 ) + ( data[ offset + 1 ] << (byte) 8 ) + ( data[ offset ] ) );
            data[ offset ] = (byte) value;
            data[ offset + 1 ] = (byte) ( value >> 8 );
            data[ offset + 2 ] = (byte) ( value >> 16 );
            data[ offset + 3 ] = (byte) ( value >> 24 );
        }
    }
}