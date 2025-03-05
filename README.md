# ApplicationDbContext
```
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<ResetPasswordRequest> resetPasswordRequests { get; set; }
}
```

# Conexão com Banco
## MySQL

appsettings.json
```
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=verify; User Id=root;Password=root;"
},
```

Program.cs
```
// Configurar o DbContext com a connection string
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    )
);
```
---

## PostgresSQL

appsettings.json
```
"ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=Product;Username=postgres;Password=0000;"
  }
```

Program.cs
```
// Configurar o DbContext com a connection string
builder.Services.AddDbContext<AplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```

# Controller

- Para criar a controller, você precisa criar as Models, o applicationDbContext, conectar ao banco de dados e fazer a migração, só assim será possível criar a controller.

    ## Criar Controller
    - ### Criando controller com sccaffold
	    1. Na pasta "Controller cria um novo item com scaffold"
	    2. Selecione o tipo "Controlador API com ações, usando o Entity Framsework"
	    3. Selecione a sua model e seu applicationDbContext, e crie o controller

# Services

Usamos services, para regras de negocio da api, isolando esses processos da controller
- Controller geralmente cuida mais da parte da entrada do dado do usuário. ex: O dados não seu nulo
- Service geralmente cuida da regra de negocio. ex: O processo de update, a parte de atualizar e se os dados na atualização forem vazios e so um deles estiverem preenchidos e atualiza apenas esse campo, enquanto o resto vai ser oq ja estava no banco, com o proposito de não deixar algum campo vazio.
