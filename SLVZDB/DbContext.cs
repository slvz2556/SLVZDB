namespace SLVZDB;

public abstract class DbContext<TModel>
{
    private string KeyName { get; set; }
    private string FilePath { get; set; }
    private Type ModelType { get; set; }

    public abstract void Configuration();
    public DbContext<TModel> SetConfig(string filepath, string keyname)
    {
        ModelType = typeof(TModel);
        FilePath = filepath;
        this.KeyName = keyname;

        return this;
    }

    protected DbContext()
    {
        Configuration();

        if (ModelType == null)
            throw new InvalidOperationException("Model type must be set in Configuration().");

        if (string.IsNullOrEmpty(KeyName) || string.IsNullOrEmpty(FilePath))
            throw new InvalidOperationException("Both KeyName and FilePath must be set in Configuration().");
    }



    public void Append(TModel model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));
        if (model.GetType() != ModelType)
            throw new InvalidOperationException($"This configuration only works with model type {ModelType.Name}.");


        try
        {
            using FileStream file = new FileStream(FilePath, File.Exists(FilePath) ? FileMode.Append : FileMode.CreateNew, FileAccess.Write);
            using (StreamWriter writer = new StreamWriter(file))
            {
                string str = "";
                var properties = ModelType.GetProperties();

                string key = ModelType.GetProperty(KeyName).GetValue(model).ToString();
                if (key.Contains("\n"))
                    throw new InvalidDataException("You cannot use Enter in key");

                str += $"<db.{KeyName}>{key}</db.{KeyName}><db.br/>";

                foreach (var prop in properties)
                {
                    if (prop.Name != KeyName)
                    {
                        string value = ModelType.GetProperty(prop.Name).GetValue(model).ToString();
                        var type = prop.PropertyType;

                        if (type == typeof(string))
                            value = value.Replace("\n", "<db.break/>");

                        str += $"<db.{prop.Name}>{value}</db.{prop.Name}><db.br/>";
                    }
                }
                if (str.EndsWith("<db.br/>"))
                    str = str.Remove(str.Length - 9, 8);

                writer.WriteLine(str);

                writer.Close();
            }

            file.Close();
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public void Append(List<TModel> models)
    {
        if (models is null)
            throw new ArgumentNullException(nameof(models));
        if (models[^1]?.GetType() != ModelType)
            throw new InvalidOperationException($"This configuration only works with model type {ModelType.Name}.");


        try
        {
            using FileStream file = new FileStream(FilePath, File.Exists(FilePath) ? FileMode.Append : FileMode.CreateNew, FileAccess.Write);
            using (StreamWriter writer = new StreamWriter(file))
            {
                foreach (var model in models)
                {
                    string str = "";
                    var properties = ModelType.GetProperties();

                    string key = ModelType.GetProperty(KeyName).GetValue(model).ToString();
                    if (key.Contains("\n"))
                        throw new InvalidDataException("You cannot use Enter in key");

                    str += $"<db.{KeyName}>{key}</db.{KeyName}><db.br/>";

                    foreach (var prop in properties)
                    {
                        if (prop.Name != KeyName)
                        {
                            string value = ModelType.GetProperty(prop.Name).GetValue(model).ToString();
                            var type = prop.PropertyType;

                            if (type == typeof(string))
                                value = value.Replace("\n", "<db.break/>");

                            str += $"<db.{prop.Name}>{value}</db.{prop.Name}><db.br/>";
                        }
                    }
                    if (str.EndsWith("<db.br/>"))
                        str = str.Remove(str.Length - 9, 8);

                    writer.WriteLine(str);
                }
                writer.Close();
            }

            file.Close();
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public List<TModel> Get()
    {
        if (ModelType == null)
            throw new InvalidOperationException("Model type is not set.");

        if (typeof(TModel) != ModelType)
            throw new InvalidOperationException($"This configuration only works with model type {ModelType.Name}.");

        List<TModel> objects = new List<TModel>();


        if (!File.Exists(FilePath))
            return objects;

        using FileStream file = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
        using (var reader = new StreamReader(file))
        {
            while (reader.Peek() != -1)
            {

                string line = reader.ReadLine();
                var instance = Activator.CreateInstance(ModelType);

                foreach (var parameter in line.Split("<db.br/>"))
                {
                    foreach (var prop in ModelType.GetProperties())
                    {
                        if (parameter.StartsWith($"<db.{prop.Name}>") && !prop.PropertyType.IsEnum)
                        {
                            if (prop.PropertyType == typeof(string))
                            {
                                string value = parameter.Replace($"<db.{prop.Name}>", "").Replace($"</db.{prop.Name}>", "").Replace("<db.break/>", "\n");
                                prop.SetValue(instance, value);
                            }
                            else
                            {
                                var value = Convert.ChangeType(parameter.Replace($"<db.{prop.Name}>", "").Replace($"</db.{prop.Name}>", ""), prop.PropertyType);
                                prop.SetValue(instance, value);
                            }
                            break;
                        }
                        else if (parameter.StartsWith($"<db.{prop.Name}>") && prop.PropertyType.IsEnum)
                        {
                            object convertedValue = parameter.Replace($"<db.{prop.Name}>", "").Replace($"</db.{prop.Name}>", "");
                            if (convertedValue is string strVal)
                                convertedValue = Enum.Parse(prop.PropertyType, strVal);
                            else
                                convertedValue = Enum.ToObject(prop.PropertyType, convertedValue);

                            prop.SetValue(instance, convertedValue);

                            break;
                        }
                    }
                }


                //Add model to the list
                objects.Add((TModel)Convert.ChangeType(instance, ModelType));
            }
            reader.Close();
        }
        file.Close();

        return objects;
    }

    public TModel Get(dynamic RecordKey)
    {
        if (ModelType == null)
            throw new InvalidOperationException("Model type is not set.");

        if (typeof(TModel) != ModelType)
            throw new InvalidOperationException($"This configuration only works with model type {ModelType.Name}.");

        var key = Activator.CreateInstance(ModelType.GetProperty(KeyName).PropertyType);
        key = Convert.ChangeType(RecordKey, key.GetType());

        var instance = Activator.CreateInstance(ModelType);

        if (!File.Exists(FilePath))
            return (TModel)Convert.ChangeType(instance, ModelType);

        using FileStream file = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
        using (var reader = new StreamReader(file))
        {
            while (reader.Peek() != -1)
            {

                string line = reader.ReadLine();

                if (line.StartsWith($"<db.{KeyName}>{key}</db.{KeyName}>"))
                {
                    foreach (var parameter in line.Split("<db.br/>"))
                    {
                        foreach (var prop in ModelType.GetProperties())
                        {
                            if (parameter.StartsWith($"<db.{prop.Name}>") && !prop.PropertyType.IsEnum)
                            {
                                if (prop.PropertyType == typeof(string))
                                {
                                    string value = parameter.Replace($"<db.{prop.Name}>", "").Replace($"</db.{prop.Name}>", "").Replace("<db.break/>", "\n");
                                    prop.SetValue(instance, value);
                                }
                                else
                                {
                                    var value = Convert.ChangeType(parameter.Replace($"<db.{prop.Name}>", "").Replace($"</db.{prop.Name}>", ""), prop.PropertyType);
                                    prop.SetValue(instance, value);
                                }

                                break;
                            }
                            else if (parameter.StartsWith($"<db.{prop.Name}>") && prop.PropertyType.IsEnum)
                            {
                                object convertedValue = parameter.Replace($"<db.{prop.Name}>", "").Replace($"</db.{prop.Name}>", "");
                                if (convertedValue is string strVal)
                                    convertedValue = Enum.Parse(prop.PropertyType, strVal);
                                else
                                    convertedValue = Enum.ToObject(prop.PropertyType, convertedValue);

                                prop.SetValue(instance, convertedValue);

                                break;
                            }
                        }
                    }
                    return (TModel)Convert.ChangeType(instance, typeof(TModel));
                }
            }
            reader.Close();
        }
        file.Close();

        return (TModel)Convert.ChangeType(instance, ModelType);
    }

    public void Remove(dynamic Key)
    {
        if (ModelType == null)
            throw new InvalidOperationException("Model type is not set.");

        var key = Activator.CreateInstance(ModelType.GetProperty(KeyName).PropertyType);
        key = Convert.ChangeType(key, key.GetType());

        if (File.Exists(FilePath))
        {
            using FileStream MainFile = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
            using FileStream TmpFile = new FileStream(FilePath + ".tmp", FileMode.CreateNew, FileAccess.Write);

            using StreamWriter writer = new StreamWriter(TmpFile);
            using StreamReader reader = new StreamReader(MainFile);

            while (reader.Peek() != -1)
            {
                string line = reader.ReadLine();
                if (!line.StartsWith($"<db.{KeyName}>{key}</db.{KeyName}>"))
                    writer.WriteLine(line);
            }

            reader.Close();
            writer.Close();

            MainFile.Close();
            TmpFile.Close();

            File.Copy(FilePath + ".tmp", FilePath, true);
            File.Delete(FilePath + ".tmp");
        }

    }

    public void Update(TModel model)
    {
        if (ModelType == null)
            throw new InvalidOperationException("Model type is not set.");

        if (model.GetType() != ModelType)
            throw new InvalidOperationException($"This configuration only works with model type {ModelType.Name}.");


        if (File.Exists(FilePath))
        {
            string key = ModelType.GetProperty(KeyName).GetValue(model).ToString();

            using FileStream MainFile = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
            using FileStream TmpFile = new FileStream(FilePath + ".tmp", FileMode.CreateNew, FileAccess.Write);

            using StreamWriter writer = new StreamWriter(TmpFile);
            using StreamReader reader = new StreamReader(MainFile);

            while (reader.Peek() != -1)
            {
                string line = reader.ReadLine();
                if (!line.StartsWith($"<db.{KeyName}>{key}</db.{KeyName}>"))
                    writer.WriteLine(line);
                else
                {
                    string str = "";
                    var properties = ModelType.GetProperties();

                    str += $"<db.{KeyName}>{key}</db.{KeyName}><db.br/>";

                    foreach (var prop in properties)
                    {
                        if (prop.Name != KeyName)
                        {
                            string value = ModelType.GetProperty(prop.Name).GetValue(model).ToString();
                            var type = prop.PropertyType;

                            if (type == typeof(string))
                                value = value.Replace("\n", "<db.break/>");

                            str += $"<db.{prop.Name}>{value}</db.{prop.Name}><db.br/>";
                        }
                    }
                    if (str.EndsWith("<db.br/>"))
                        str = str.Remove(str.Length - 9, 8);

                    writer.WriteLine(str);
                }
            }

            reader.Close();
            writer.Close();

            MainFile.Close();
            TmpFile.Close();

            File.Copy(FilePath + ".tmp", FilePath, true);
            File.Delete(FilePath + ".tmp");
        }

    }
}

//SLVZ