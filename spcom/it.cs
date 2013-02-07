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
    /// <summary>
    /// A complete it (Impulse Tracker) file stored in memory as an instance of this class.
    /// </summary>
    partial class it {
        /// <summary>
        /// Reduces the number of channels from 16 to 8, replacing empty spaces with pan commands as the pan calculated from volume command pairs (from channels 1 & 9, 2 & 10, etc) changes.
        /// </summary>
        public void reduceChannels() {

            if ( channels != 16 )
                throw new FormatException( "Number of channels must be 16 for '-channels' to work." );

            for ( int i = 0; i < numpatterns; i++ ) {
                byte[] prevpan = new byte[ 8 ];
                for ( int row = 0; row < patterns[ i ].rows; row++ ) {
                    for ( int channel = 0; channel < 8; channel++ ) {
                        byte vol1 = patterns[ i ].row[ row ].channel[ channel ].volume;
                        byte vol2 = patterns[ i ].row[ row ].channel[ channel + 8 ].volume;
                        if ( ( vol1-- != 0 ) && ( vol2-- != 0 ) ) {
                            byte pan = get_pan( vol1, vol2 );
                            if ( pan != prevpan[ channel ] ) {
                                if ( Math.Abs( pan - prevpan[ channel ] ) > 4 ) {
                                    patterns[ i ].row[ row ].channel[ channel ].command = 0x18;
                                    patterns[ i ].row[ row ].channel[ channel ].parameter = pan;
                                    prevpan[ channel ] = pan;
                                }
                            }
                            if ( vol2 > vol1 )
                                patterns[ i ].row[ row ].channel[ channel ].volume = ++vol2;

                            double divisionfactor = 0.5;
                            byte workingpanb;
                            if ( ( patterns[ i ].row[ row ].channel[ channel ].command == 0x18 ) && ( patterns[ i ].row[ row ].channel[ channel ].parameter != 0 ) )
                                workingpanb = patterns[ i ].row[ row ].channel[ channel ].parameter;

                            else
                                workingpanb = prevpan[ channel ];

                            if ( workingpanb > 0x80 ) {
                                unchecked {
                                    workingpanb = (byte) ~( workingpanb );
                                }
                            }
                            float workingpan=workingpanb;
                            if ( workingpan == 0 )
                                workingpan++;
                            double ampfactor = ( ( 1 - divisionfactor ) * ( workingpan / 0x80 ) ) + divisionfactor;
                            double newvol = --patterns[ i ].row[ row ].channel[ channel ].volume;
                            newvol *= ampfactor;
                            patterns[ i ].row[ row ].channel[ channel ].volume = (byte) ( newvol + 1 );
                        }
                    }
                }
            }

            reduceHeaderChannelCount( 8 );
        }

        /// <summary>
        /// Returns a parameter for the pan command based on 2 volume commands given.
        /// </summary>
        /// <param name="vl1"></param>
        /// <param name="vl2"></param>
        /// <returns></returns>
        byte get_pan( byte vl1, byte vl2 ) {
            float pan=0x80;
            float v1=(float) vl1 * 2;
            float v2=(float) vl2 * 2;
            if ( v1 > 0 && v2 > 0 ) {
                if ( v1 > v2 ) {
                    pan = ( v2 / v1 ) * 0x80;
                }
                else if ( v2 > v1 ) {
                    pan = ~( (byte) ( ( v1 / v2 ) * 0x80 ) );
                }
            }
            else {
                if ( v1 == 0 )
                    pan = 0xFF;
                if ( v2 == 0 )
                    pan = 0x00;
                if ( v1 == 0 && v2 == 0 )
                    pan = 0x80;
            }
            return (byte) pan;
        }

        /// <summary>
        /// Reduces the number of channels as specified in the file header in the form of channel pan values. Intended to drop channel count from 16 to 8.
        /// </summary>
        /// <param name="channels"></param>
        private void reduceHeaderChannelCount( byte channels ) {
            if ( channels > 64 )
                throw new ArgumentException( "reduceHeaderChannelCount(): Number of channels specified greater than 64." );
            this.channels = channels;
            for ( int i = 0; i < channels; i++ ) {
                channelpan[ i ] = 0x20;
            }
            for ( int i = channels; i < 64; i++ ) {
                channelpan[ i ] = 0x80;
            }
        }

        /// <summary>
        /// Decrease the C5 frequency of each sample by about 30-40 cents to compensate for whatever bug plauged OpenSPC, left unfixed by its tone-deaf programmer...
        /// </summary>
        public void fixPitch() {
            for ( int i = 0; i < numsamples; i++ ) {
                if ( samples[ i ].length == 0 )
                    continue;
                double original = (double) samples[ i ].c5speed;
                // see 'pitch calculations.xlsx'
                samples[ i ].c5speed = (uint) ( original * Math.Pow( 2, ( -5.393 * Math.Log( original ) + 14.228 ) / 1200 ) );
            }
        }

        private uint BIT( int bit ) {
            return (uint) ( 1 << bit );
        }
    }
}
