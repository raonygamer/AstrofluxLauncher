package {
import flash.filesystem.File;
import flash.filesystem.FileMode;
import flash.filesystem.FileStream;

public class Log {
    private var logStream:FileStream = new FileStream();

    function Log(file:File) {
        logStream.open(file, FileMode.APPEND);
        debug("Logging started at " + new Date().toString());
    }

    public function debug(...args):void {
        trace(args);
        if (args.length == 0) {
            return;
        }
        logStream.writeUTFBytes(args.join(" ") + "\n");
    }

    public static function create(file:File):Log {
        if (file == null) {
            throw new Error("File cannot be null");
        }

        if (!file.exists) {
            if (!file.parent.exists) {
                file.parent.createDirectory();
            }
        }
        else {
            file.deleteFile();
        }

        return new Log(file);
    }
}
}