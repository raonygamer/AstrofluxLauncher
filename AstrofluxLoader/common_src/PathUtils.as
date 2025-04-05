package {
public class PathUtils {
    public static function getFileName(path:String):String {
        if (path == null) {
            throw new Error("Path cannot be null");
        }
        var lastSlashIndex:int = Math.max(path.lastIndexOf("/"), path.lastIndexOf("\\"));
        if (lastSlashIndex == -1) {
            return path;
        }
        return path.substring(lastSlashIndex + 1);
    }
}
}