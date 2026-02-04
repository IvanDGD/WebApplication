using System.IO.Compression;

public class ImagesZipMiddleware
{
    readonly RequestDelegate next;
    readonly string imagesPath;

    public ImagesZipMiddleware(RequestDelegate next, IWebHostEnvironment env)
    {
        this.next = next;
        imagesPath = Path.Combine(env.ContentRootPath, "images");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string path = context.Request.Path;

        if (path == "/images")
        {
            string filesQuery = context.Request.Query["files"];

            if (string.IsNullOrWhiteSpace(filesQuery))
            {
                context.Response.StatusCode = 400;
                return;
            }

            string[] fileNames = filesQuery.Split(',');

            await using MemoryStream zipStream = new MemoryStream();

            using (ZipArchive zip = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                foreach (string fileName in fileNames)
                {
                    string filePath = Path.Combine(imagesPath, fileName);

                    if (!File.Exists(filePath))
                        continue;

                    ZipArchiveEntry entry = zip.CreateEntry(fileName);

                    await using Stream entryStream = entry.Open();
                    await using FileStream fileStream = File.OpenRead(filePath);

                    await fileStream.CopyToAsync(entryStream);
                }
            }

            zipStream.Position = 0;

            context.Response.ContentType = "application/zip";
            context.Response.Headers["Content-Disposition"] =
                "attachment; filename=images.zip";

            await zipStream.CopyToAsync(context.Response.Body);
        }
        else
        {
            await next(context);
        }
    }
}
