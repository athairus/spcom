using System;
using System.IO;

namespace spcom {
    /// <summary>
    /// Filesystem interface, loads files into instances of the <code>it</code> class.
    /// </summary>
    static class io {
        // max 64MB file, any larger and the parse method should fail
        public static int maxSize = 1024 * 1024 * 64;

        /// <summary>
        /// Loads an it file into memory, calls <code>it.parse()</code>, then closes the file.
        /// </summary>
        /// <param name="filename">Full path to the file being loaded.</param>
        /// <returns>An instance of the <code>it</code> class, fully loaded with all the information contained in the file.</returns>
        public static it load( string filename ) {

            // check if file exists and load it
            FileStream file = null;
            try {
                file = new FileStream( filename, FileMode.Open, FileAccess.Read, FileShare.Read );
            }
            catch ( Exception e ) {
                throw new IOException( "(opening in file stream): " + e.Message );
            }

            // load file into byte array
            byte[] data = new byte[ maxSize ];
            try {
                file.Read( data, 0, maxSize );
            }
            catch ( Exception e ) {
                file.Close();
                throw new IOException( "(reading in file stream): " + e.Message );
            }
            it itfile = new it();

            // close the file
            file.Close();

            try {
                itfile.parse( data );
            }
            catch ( Exception e ) {
                throw new IOException( "it.parse(): " + e.Message );
            }

            return itfile;
        }
        public static void write( it song, string filename ) {
            byte[] data;
            try {
                data = song.pack();
            }
            catch ( Exception e ) {
                throw new IOException( "it.pack(): " + e.Message );
            }

            FileStream file = null;
            try {
                file = new FileStream( filename, FileMode.Create, FileAccess.Write, FileShare.Write);
            }
            catch ( Exception e ) {
                file.Close();
                throw new IOException( "(opening out file stream): " + e.Message );
            }

            try {
                file.Write( data, 0, data.Length );
            }
            catch ( Exception e ) {
                file.Close();
                throw new IOException( "(writing out file stream): " + e.Message );
            }
            return;
        }
    }
}
