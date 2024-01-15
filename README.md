Zippy is a little benchmark application to test the write and read speed differences between a ZipArchive and the File System when write a large number of small files.

![image](https://github.com/okmer/ZippyArchiveVsFileSystem/assets/3484773/94d6daa5-f1ee-4e4d-a9c3-2c2778421ed2)
The ZipArchive seems to be a clear winner in the fast SSD of my laptop, while writen 50.000 x 50KByte files.

![image](https://github.com/okmer/ZippyArchiveVsFileSystem/assets/3484773/5557fe2c-64b1-4637-ba5c-73a7f88a2f3e)
The ZipArchive (NoCompression) is a lot lighter on the CPU, then the slower 20 parallel file system files.
