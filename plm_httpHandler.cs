//------
//Код серверного HTTP обработчика
//Ключ - LibraryFilesDownLoader_http
//Имя - [Программа получения всех библиотечных файлов в виде архива] http-обработчик
//Конфигурация - Программа получения всех библиотечных файлов в виде архива
//------


/// <summary>
/// Обработчик веб запросов. Выполняется в серверных потоках-обработчиках сетевых запросов
/// </summary>
/// <param name="context">HTTP контекст запроса</param>
/// <param name="session">Сессия Web пользователя. Используйте session.User == null, если пользователь не известен.</param>
public override bool HandleWebRequest( HttpListenerContext context, WebUserSession session )
{
	HttpListenerRequest request = context.Request;
	HttpListenerResponse response = context.Response;
	
	string Url = request.Url.LocalPath;
    if (Url != "/LibraryFilesDownLoader")
    {
        return false;
    } 
    
    MemoryStream zipStream = (MemoryStream)Service.GetScriptingTask("LibraryFilesDownLoader_clientFunction").Invoke();
    if(zipStream == null)
    {
	    string res = "Не удалось собрать архив.";
	    
	    byte[] buffer = Encoding.UTF8.GetBytes( res );
		response.ContentLength64 = buffer.Length;
		response.ContentEncoding = Encoding.UTF8;
		response.Headers.Add(HttpResponseHeader.CacheControl, "no-cache, no-store, must-revalidate");
		response.Headers.Add(HttpResponseHeader.Pragma, "no-cache");
		response.Headers.Add(HttpResponseHeader.Expires, "0");
	
		response.Close( buffer, true );
		return true;
    }
    
    var OutputFilename = @"result.zip";
   	byte[] bb = zipStream.ToByteArray();
	
	response.Headers.Add(HttpResponseHeader.Pragma, "no-cache;");
	response.Headers.Add(HttpResponseHeader.Expires, "0");
	response.Headers.Add(HttpResponseHeader.CacheControl, "must-revalidate, post-check=0, pre-check=0;");
	response.Headers.Add(HttpResponseHeader.CacheControl, "public;");
	response.Headers.Add(@"Content-Description: File Transfer;");
	response.Headers.Add(@"Content-Type: application/octet-stream;");
	response.Headers.Add(@"Content-Disposition: attachment; filename="+OutputFilename+";");
	response.Headers.Add(@"Content-Transfer-Encoding: binary;");
	response.ContentLength64 = bb.Length;

    response.Close(bb, true);
	return true;
}
