using System;

namespace spcom {
    partial class it {
        /// <summary>
        /// Attempts to parse the <code>byte</code> array as an it file, populating the instance of the class this function was called from with data.
        /// </summary>
        /// <param name="data"><code>byte</code> array that contains the data, which should be a complete .it file. </param>
        /// <throws>FormatException</throws>
        public void parse( byte[] data ) {
            #region Parse the header

            magicword = read4( data, 0x00 );
            if ( magicword != 0x4D504D49 )
                throw new FormatException( "Magic word mismatch. File must begin with the letters \"IMPM\". Are you sure this is an it file?" );

            songname = System.Text.Encoding.ASCII.GetString( data, 0x04, 26 );

            pathighlight = read2( data, 0x1E );

            numorders = read2( data, 0x20 );

            insnum = read2( data, 0x22 );
            if ( insnum != 0 )
                throw new FormatException( "Instruments are detected, they are not supported. This program was made for it files that were made using OpenSPC." );

            numsamples = read2( data, 0x24 );

            numpatterns = read2( data, 0x26 );

            createdwithtracker = read2( data, 0x28 );

            compatablewithtracker = read2( data, 0x2A );

            flags = read2( data, 0x2C );

            special = read2( data, 0x2E );

            globalvol = data[ 0x30 ];

            mixvol = data[ 0x31 ];

            initialspeed = data[ 0x32 ];

            initialtempo = data[ 0x33 ];

            panseperation = data[ 0x34 ];

            pitchwheeldepth = data[ 0x35 ];

            messagelength = read2( data, 0x36 );

            messageoffset = read4( data, 0x38 );
            if ( messagelength > 8000 )
                throw new FormatException( "Message length is greater than 8000 bytes." );
            message = new byte[ messagelength ];
            for ( int i = 0; i < messagelength; i++ ) {
                message[ i ] = data[ messageoffset + i ];
            }


            reserved = read4( data, 0x3C );

            channelpan = new byte[ 64 ];
            channels = 0;
            for ( int i = 0; i < 64; i++ ) {
                channelpan[ i ] = data[ 0x40 + i ];
                if ( channelpan[ i ] < 128 )
                    channels++;
            }

            channelvol = new byte[ 64 ];
            for ( int i = 0; i < 64; i++ ) {
                channelvol[ i ] = data[ 0x80 + i ];
            }

            orders = new byte[ numorders ];
            for ( int i = 0; i < numorders; i++ ) {
                orders[ i ] = data[ 0xC0 + i ];
            }

            // ignoring instruments, since they're not supported there shouldn't be any to read

            sampleoffsets = new uint[ numsamples ];
            for ( int i = 0; i < numsamples; i++ ) {
                sampleoffsets[ i ] = read4( data, 0xC0 + numorders + i * 4 );
            }

            patternoffsets = new uint[ numpatterns ];
            for ( int i = 0; i < numpatterns; i++ ) {
                patternoffsets[ i ] = read4( data, 0xC0 + numorders + numsamples * 4 + i * 4 );
            }

            #endregion

            #region Parse the samples

            samples = new itsample[ numsamples ];
            try {
                for ( int i = 0; i < numsamples; i++ ) {

                    samples[ i ].magicword = read4( data, sampleoffsets[ i ] + 0x00 );
                    if ( samples[ i ].magicword != 0x53504D49 )
                        throw new FormatException( String.Format( "Magic word mismatch on sample #{0}, offset 0x{1:X}. Header must begin with the letters \"IMPS\". File may be corrupt.", i, sampleoffsets[ i ] ) );

                    samples[ i ].dosfilename = System.Text.Encoding.ASCII.GetString( data, (int) sampleoffsets[ i ] + 0x04, 12 );

                    samples[ i ].globalvol = data[ sampleoffsets[ i ] + 0x11 ];

                    samples[ i ].flags = data[ sampleoffsets[ i ] + 0x12 ];

                    samples[ i ].defaultvol = data[ sampleoffsets[ i ] + 0x13 ];

                    samples[ i ].samplename = System.Text.Encoding.ASCII.GetString( data, (int) sampleoffsets[ i ] + 0x14, 26 );

                    samples[ i ].convert = data[ sampleoffsets[ i ] + 0x2E ];

                    samples[ i ].defaultpan = data[ sampleoffsets[ i ] + 0x2F ];

                    samples[ i ].length = read4( data, sampleoffsets[ i ] + 0x30 );
                    if ( samples[ i ].length > ( 64 * 1024 * 1024 ) )
                        throw new FormatException( String.Format( "Sample size for sample #{0} is greater than 64MB (according to the header, it's {1}MB), aborting to avoid excessive memory usage.", i, samples[ i ].length / 1024 / 1024 ) );

                    samples[ i ].loopbegin = read4( data, sampleoffsets[ i ] + 0x34 );

                    samples[ i ].loopend = read4( data, sampleoffsets[ i ] + 0x38 );

                    samples[ i ].c5speed = read4( data, sampleoffsets[ i ] + 0x3C );

                    samples[ i ].susloopbegin = read4( data, sampleoffsets[ i ] + 0x40 );

                    samples[ i ].susloopend = read4( data, sampleoffsets[ i ] + 0x44 );

                    samples[ i ].samplepointer = read4( data, sampleoffsets[ i ] + 0x48 );

                    samples[ i ].vibratospeed = data[ sampleoffsets[ i ] + 0x4C ];

                    samples[ i ].vibratodepth = data[ sampleoffsets[ i ] + 0x4D ];

                    samples[ i ].vibratorate = data[ sampleoffsets[ i ] + 0x4E ];

                    samples[ i ].vwaveformtype = data[ sampleoffsets[ i ] + 0x4F ];

                    samples[ i ].sample = new byte[ samples[ i ].length * 2 ];
                    for ( int j = 0; j < samples[ i ].length * 2; j++ ) {
                        samples[ i ].sample[ j ] = data[ samples[ i ].samplepointer + j ];
                    }
                }
            }
            catch ( Exception e ) {
                throw e;
            }

            #endregion

            #region Parse the patterns

            patterns = new itpattern[ numpatterns ];
            try {
                for ( int i = 0; i < numpatterns; i++ ) {

                    patterns[ i ].length = read2( data, patternoffsets[ i ] + 0x00 );

                    patterns[ i ].rows = read2( data, patternoffsets[ i ] + 0x02 );

                    patterns[ i ].packeddata = new byte[ patterns[ i ].length ];
                    for ( int j = 0; j < patterns[ i ].length; j++ ) {
                        patterns[ i ].packeddata[ j ] = data[ patternoffsets[ i ] + 0x08 + j ];
                    }

                    patterns[ i ].row = new rowgroup[ patterns[ i ].rows ];

                    for ( int row = 0; row < patterns[ i ].rows; row++ ) {
                        patterns[ i ].row[ row ].channel = new channelgroup[ channels ];
                        for ( int channel = 0; channel < channels; channel++ ) {
                            patterns[ i ].row[ row ].channel[ channel ].note = 0;
                            patterns[ i ].row[ row ].channel[ channel ].instrument = 0;
                            patterns[ i ].row[ row ].channel[ channel ].volume = 0;
                            patterns[ i ].row[ row ].channel[ channel ].command = 0;
                            patterns[ i ].row[ row ].channel[ channel ].parameter = 0;
                        }
                    }

                    // now, unpack the data
                    channelgroup[] prev = new channelgroup[ 64 ];
                    byte[] channelmask = new byte[ 64 ];
                    byte[] maskvar = new byte[ 64 ];

                    ushort rowsize = (ushort) ( channels * 5 );
                    ushort offset = 0;
                    byte currentchannel = 0;
                    int currentrow = 0;

                    try {
                        while ( currentrow < patterns[ i ].rows ) {
                            if ( offset >= patterns[ i ].packeddata.Length ) { // sanity check
                                throw new FormatException( String.Format( "Error parsing pattern: offset > length, pattern #{0}, offset = {1:X} length = {2:X}. The file may be corrupt.", i, offset, patterns[ i ].length ) );
                                //break;
                            }
                            byte channelvar = patterns[ i ].packeddata[ offset++ ];

                            if ( channelvar == 0 ) {
                                currentrow++;
                                continue;
                            }

                            if ( currentrow > 255 ) {
                                throw new FormatException( String.Format( "Error parsing pattern: currentrow > 255, pattern #{0}. The file may be corrupt.", i ) );
                                //break;
                            }

                            currentchannel = (byte) ( ( channelvar - 1 ) & 63 );

                            if ( ( channelvar & 128 ) == 128 ) // will not be updated if the highest bit is not set, will instead use whatever's already there at that position
                                maskvar[ currentchannel ] = patterns[ i ].packeddata[ offset++ ];

                            if ( ( maskvar[ currentchannel ] & 1 ) == 1 ) { // note command
                                byte note = patterns[ i ].packeddata[ offset++ ];
                                if ( note < 0xFD )
                                    note++;
                                //patterns[ i ].unpackeddata[ currentrow * rowsize + currentchannel * 5 + 0 ] = note;
                                patterns[ i ].row[ currentrow ].channel[ currentchannel ].note = note;
                                prev[ currentchannel ].note = note;
                            }
                            if ( ( maskvar[ currentchannel ] & 2 ) == 2 ) { // instrument command
                                byte instrument = patterns[ i ].packeddata[ offset++ ];
                                //patterns[ i ].unpackeddata[ currentrow * rowsize + currentchannel * 5 + 1 ] = instrument;
                                patterns[ i ].row[ currentrow ].channel[ currentchannel ].instrument = instrument;
                                prev[ currentchannel ].instrument = instrument;
                            }
                            if ( ( maskvar[ currentchannel ] & 4 ) == 4 ) { // volume command
                                byte volume = (byte) ( patterns[ i ].packeddata[ offset++ ] + 1 );
                                //patterns[ i ].unpackeddata[ currentrow * rowsize + currentchannel * 5 + 2 ] = volume;
                                patterns[ i ].row[ currentrow ].channel[ currentchannel ].volume = volume;
                                prev[ currentchannel ].volume = volume;
                            }
                            if ( ( maskvar[ currentchannel ] & 8 ) == 8 ) { // effect command
                                byte command = patterns[ i ].packeddata[ offset++ ];
                                byte parameter = patterns[ i ].packeddata[ offset++ ];
                                if ( command != 0 ) {
                                    //patterns[ i ].unpackeddata[ currentrow * rowsize + currentchannel * 5 + 3 ] = command;
                                    patterns[ i ].row[ currentrow ].channel[ currentchannel ].command = command;
                                    //patterns[ i ].unpackeddata[ currentrow * rowsize + currentchannel * 5 + 4 ] = parameter;
                                    patterns[ i ].row[ currentrow ].channel[ currentchannel ].parameter = parameter;
                                    prev[ currentchannel ].command = command;
                                    prev[ currentchannel ].parameter = parameter;
                                }
                            }
                            if ( ( maskvar[ currentchannel ] & 16 ) == 16 ) {  // previous note command
                                //patterns[ i ].unpackeddata[ currentrow * rowsize + currentchannel * 5 + 0 ] = prev[ currentchannel ].note;
                                patterns[ i ].row[ currentrow ].channel[ currentchannel ].note = prev[ currentchannel ].note;
                            }
                            if ( ( maskvar[ currentchannel ] & 32 ) == 32 ) { // previous instrument command
                                //patterns[ i ].unpackeddata[ currentrow * rowsize + currentchannel * 5 + 1 ] = prev[ currentchannel ].instrument;
                                patterns[ i ].row[ currentrow ].channel[ currentchannel ].instrument = prev[ currentchannel ].instrument;
                            }
                            if ( ( maskvar[ currentchannel ] & 64 ) == 64 ) { // previous volume command
                                //patterns[ i ].unpackeddata[ currentrow * rowsize + currentchannel * 5 + 2 ] = prev[ currentchannel ].volume;
                                patterns[ i ].row[ currentrow ].channel[ currentchannel ].volume = prev[ currentchannel ].volume;
                            }
                            if ( ( maskvar[ currentchannel ] & 128 ) == 128 ) { // previous effect command
                                //patterns[ i ].unpackeddata[ currentrow * rowsize + currentchannel * 5 + 3 ] = prev[ currentchannel ].command;
                                patterns[ i ].row[ currentrow ].channel[ currentchannel ].command = prev[ currentchannel ].command;
                                //patterns[ i ].unpackeddata[ currentrow * rowsize + currentchannel * 5 + 4 ] = prev[ currentchannel ].parameter;
                                patterns[ i ].row[ currentrow ].channel[ currentchannel ].parameter = prev[ currentchannel ].parameter;
                            }

                        }
                    }
                    catch ( Exception e ) {
                        throw e;
                    }




                }
            }
            catch ( Exception e ) {
                throw e;
            }

            #endregion

            
        }

        private ushort read2( byte[] data, long offset ) {
            return (ushort) ( ( data[ offset + 1 ] << (byte) 8 ) + ( data[ offset ] ) );
        }
        private uint read4( byte[] data, long offset ) {
            return (uint) ( ( data[ offset + 3 ] << (byte) 24 ) + ( data[ offset + 2 ] << (byte) 16 ) + ( data[ offset + 1 ] << (byte) 8 ) + ( data[ offset ] ) );
        }
    }
}