using SLVZDB;

// <---  SLVZ.DEV  --->

var db = new myDB();

//Add 3 users
db.Append(new myModel
{
    ID = 1,
    Name = "Ashkan",
    Grade = 19.234,
    IsActive = true,
    Role = Role.Admin
});
db.Append(new myModel
{
    ID = 2,
    Name = "Arash",
    Grade = 21.992,
    IsActive = true,
    Role = Role.User
});
db.Append(new myModel
{
    ID = 3,
    Name = "Baran",
    Grade = 18.224452,
    IsActive = false,
    Role = Role.User
});



//Get all models
var models = db.Get();


//Change one of the models
models.Find(x => x.ID == 3).IsActive = true;


//Save that model
db.Update(models.Find(x => x.ID == 3));


//Delete model
//db.Remove("3");


//Find one item by it ID
var model = db.Get(3);


Console.WriteLine("Done");





class myDB : DbContext<myModel>
{
    public override void Configuration()
    {
        SetConfig($"{Environment.CurrentDirectory}/slvz.db", "ID");
                  //Database full path                      //Variable name to use as ID to find models
    }
}



enum Role
{
    Admin,
    User
}
class myModel
{
    public int ID { get; set; }
    public string Name { get; set; }
    public Role Role { get; set; }
    public double Grade { get; set; }
    public bool IsActive { get; set; }
}