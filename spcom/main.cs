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

/// <summary>
/// Main console-mode program code for spcom.
/// </summary>
namespace spcom {
    class main {
        static int Main( string[] args ) {

            string usage = "Usage: spcom infile outfile [-pitch] [-channels]";

            if ( args.Length < 2 ) {
                Console.WriteLine( usage );
                return 0;
            }

            it song = null;
            try {
                song = io.load( args[ 0 ] );
            }
            catch ( Exception e ) {
                Console.WriteLine( "io.load(): " + e.Message );
                return 1;
            }

            bool pitch = false;
            bool channels = false;
            for ( int i = 2; i < args.Length; i++ ) {
                if ( args[ i ] == "-pitch" && pitch == false ) {
                    try {
                        song.fixPitch();
                    }
                    catch ( Exception e ) {
                        Console.WriteLine( "song.fixPitch(): " + e.Message );
                    }
                    pitch = true;
                }

                if ( args[ i ] == "-channels" && channels == false ) {
                    try {
                        song.reduceChannels();
                    }
                    catch ( Exception e ) {
                        Console.WriteLine( "song.reduceChannels(): " + e.Message );
                    }
                    channels = true;
                }
            }

            try {
                io.write( song, args[ 1 ] );
            }
            catch ( Exception e ) {
                Console.WriteLine( "io.write: " + e.Message );
                return 1;
            }
            return 0;
        }
    }
}
