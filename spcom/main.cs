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
