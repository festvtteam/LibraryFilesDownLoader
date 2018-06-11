//------
//Код UI-команды для АРМ разработчик
//Ключ - LibraryFilesDownLoader_clientFunction
//Имя - Скачать все библиотечные файлы
//Конфигурация - Программа получения всех библиотечных файлов в виде архива.
//------


/// <summary>
/// Проверка применимости данной команды в текущем окружении
/// </summary>
/// <returns>Возвращает true, если данная команда потенциально может быть применена</returns>
/// <remarks>Вызывается при первом построении меню или тулбара объекта</remarks>
public override bool IsValid()
{
	return true;
}

/// <summary>
/// Обновление состояния элементов интерфейса в соответствие с текущим состоянием объекта
/// </summary>
/// <param name="obj">Выделенный объект,к которому потенциально может быть применена команда</param>
/// <param name="cmdUI">Объект для управления контролом вызова команды</param>
public override void UpdateState( InfoObject obj, ICmdUI cmdUI )
{
	bool flg = false;
	if(getSTask()!=null)
		flg = true;
	cmdUI.Enabled = flg;
	cmdUI.Visible = flg;
}

/// <summary>
/// Выполнение команды над указанным объектом
/// </summary>
/// <param name="obj">Объект, к которому следует применить команду</param>
public override void Invoke( InfoObject obj )
{
	MemoryStream zipStream = (MemoryStream)getSTask().Invoke();
	if(zipStream == null) 
	{
		MessageBox.Show("Не удалось собрать архив");
		return;
	}
	var OutputFilename = @"result.zip";
	try
	{
		using (var folderBrowser = new FolderBrowserDialog())
	    {
	    	folderBrowser.Description = "Выберите папку для выгрузки документов со структурой";
	        
	        if (folderBrowser.ShowDialog() != DialogResult.OK)
	            return;
	
	        var basePath = folderBrowser.SelectedPath;
	        var folder = new DirectoryInfo(basePath);
	        
	        using(var zip = new System.IO.Compression.ZipArchive(zipStream, System.IO.Compression.ZipArchiveMode.Read))
			{
				foreach (System.IO.Compression.ZipArchiveEntry entry in zip.Entries)
				{
					using(var file = File.Create(folder.FullName + "\\" + entry.FullName))
					{
						Stream strm = entry.Open();
						CopyStream(strm, file);				            
						strm.Close();
						strm.Dispose();
					}
				}
			}
	    }
	}
	catch(Exception e)
	{
		zipStream.Close();
		zipStream.Dispose();
	}
}

private ScriptingTask getSTask()
{
	return Service.GetScriptingTask("LibraryFilesDownLoader_clientFunction");
}

private static void CopyStream(Stream src, Stream dest)
{
    var buffer = new byte[8192];
    
    for(;;)
    {
        int numRead = src.Read(buffer, 0, buffer.Length);
        
        if(numRead == 0)
            break;
        
        dest.Write(buffer, 0, numRead);
    }
}

