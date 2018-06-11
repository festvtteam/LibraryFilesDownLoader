//------
//Код клиентской скриптовой функции
//Ключ - LibraryFilesDownLoader_clientFunction
//Имя - [Программа получения всех библиотечных файлов в виде архива] клиентская функция
//Конфигурация - Программа получения всех библиотечных файлов в виде архива
//------

/// <summary>
/// Вызов метода, реализованного скриптовой функцией
/// </summary>
/// <param name="session">Пользовательская сессия, инициировавшая вызов 
/// (может быть null, если вызов инициировал серверный скрипт без указания сессии)</param>
/// <param name="inputParams">Сериализуемые входные данные для метода</param>
/// <returns>Результат работы метода</returns>
public override Object Invoke( UserSession session, Object inputParams )
{
	//ищем все библиотечные файлы
	var searchOperation = new SearchOperation( EntityIdentifier.InfoObject );
	var template1 = Service.GetTemplate( AttributableEntityIdentifier.InfoObject, @"InfoObjects\ReferenceBooks\DatabaseOfERComponent\LibraryOfPCAD\ObjectsOfStorage\PCADLibraryFile" );
	
	SearchExpressionItem rootItem;
	{
		var templateFilter1 = new SearchTemplateFilterItem(  );
		templateFilter1.FilterByTemplate = template1;
		templateFilter1.IncludeInherited = true;
		rootItem = templateFilter1;
	}
	
	searchOperation.SearchExpressionTree = rootItem;
	
	searchOperation.Execute( false );
	var resultObjects = searchOperation.FoundObjects;
	
	//получаем словарь "имя файла"-"тело файла"
	Dictionary<string, Stream> dic = new Dictionary<string, Stream>();
	
	foreach(InfoObject io in resultObjects)
	{
		FileDesc fDesc = io.GetValue<FileDesc>("LibraryFile");
		if(fDesc == null) continue;
		string fName = fDesc.OriginalName;
		while(true)
		{
			if(dic.ContainsKey(fName))
				fName+="_1";
			else
				break;
		}
		Stream strm = fDesc.DownloadFile();
		MemoryStream tempMs = new MemoryStream();
		strm.CopyTo(tempMs);
		strm.Close();
		strm.Dispose();
		tempMs.Seek(0, SeekOrigin.Begin);
		dic.Add(fName, tempMs);
	}
	
	if(dic.Count() == 0) return null;
	
	
	MemoryStream zipStream = new MemoryStream();
	
	try
	{
		//новый архив
	    System.IO.Compression.ZipArchive archive = new System.IO.Compression.ZipArchive(zipStream, System.IO.Compression.ZipArchiveMode.Create, true);
	    foreach(KeyValuePair<string, Stream> kvp in dic)
	    {
	    	var zipArchiveEntry = archive.CreateEntry(kvp.Key, System.IO.Compression.CompressionLevel.Optimal);
	    	using(var zipStreamOpn = zipArchiveEntry.Open())
	    	{
	    		kvp.Value.CopyTo(zipStreamOpn);
	    		zipStreamOpn.Close();
	    		zipStreamOpn.Dispose();
	    		kvp.Value.Close();
				kvp.Value.Dispose();
	    	}
	    }
	 	archive.Dispose();	
	 }
	 catch(Exception e)
	 {
	 	foreach(KeyValuePair<string, Stream> kvp in dic)
	    {
	    	kvp.Value.Close();
	    	kvp.Value.Dispose();
	    }
	 }
	    
    zipStream.Seek(0, SeekOrigin.Begin);  
    
    return zipStream;
}