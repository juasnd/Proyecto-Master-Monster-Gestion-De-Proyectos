namespace PROYECTO_MONGO_DOTNET.Services;

public sealed class CodigoDAO(SqlContexto db) : SqlDAO(db)
{
    public string GenerarCodigo(string tabla,string campo)
    {
        var c=Configuracion(tabla,campo);
        using var cn=Db.Abrir(); using var tx=cn.BeginTransaction(System.Data.IsolationLevel.Serializable);
        using var cmd=cn.CreateCommand(); cmd.Transaction=tx;
        cmd.CommandText="UPDATE app.Codigo WITH (UPDLOCK,HOLDLOCK) SET Ultimo=Ultimo+1 OUTPUT inserted.Prefijo,inserted.Longitud,inserted.Ultimo WHERE Tabla=@Tabla AND Campo=@Campo;";
        cmd.Parameters.AddWithValue("@Tabla",c.Tabla);cmd.Parameters.AddWithValue("@Campo",c.Campo);
        using var rd=cmd.ExecuteReader(); string prefijo;int longitud,ultimo;
        if(rd.Read()){prefijo=rd.GetString(0);longitud=rd.GetInt32(1);ultimo=rd.GetInt32(2);rd.Close();}
        else{rd.Close();cmd.CommandText="INSERT app.Codigo(Tabla,Campo,Prefijo,Longitud,Ultimo,Descripcion) VALUES(@Tabla,@Campo,@Prefijo,@Longitud,1,@Descripcion)";cmd.Parameters.AddWithValue("@Prefijo",c.Prefijo);cmd.Parameters.AddWithValue("@Longitud",c.Longitud);cmd.Parameters.AddWithValue("@Descripcion","Código autogenerable de "+c.Tabla);cmd.ExecuteNonQuery();prefijo=c.Prefijo;longitud=c.Longitud;ultimo=1;}
        tx.Commit(); return prefijo+ultimo.ToString().PadLeft(Math.Max(1,longitud-prefijo.Length),'0');
    }
    private static (string Tabla,string Campo,string Prefijo,int Longitud) Configuracion(string tabla,string campo)
    { tabla=tabla.Trim().ToLowerInvariant();campo=campo.Trim().ToUpperInvariant();return campo switch {"PEDEP_CODIGO"=>("pedep_depart",campo,"DEP",10),"PECAR_CODIGO"=>("pecar_cargo",campo,"CAR",10),"PEPER_CODIGO"=>("peper_person",campo,"PER",10),"PEEMP_CODIGO"=>("peemp_emplea",campo,"EMP",10),"PEFAM_CODIGO"=>("pefam_famil",campo,"FAM",10),"PEFOR_CODIGO"=>("pefor_formac",campo,"FOR",10),"XERHI_CODIGO"=>("xerep_reporte_historial",campo,"RHI",10),"CODIGO" when tabla=="gepro_proyec"=>("gepro_proyec",campo,"PRO",10),"CODIGOASIGNACION"=>("ge_peemp_gepro",campo,"ASI",10),"CODIGOREGISTRO"=>("gepro_registro_horas",campo,"HOR",10),"CODIGOSEGUIMIENTO"=>("gepro_seguimiento",campo,"SEG",10),"CODIGOFASE"=>("gepro_fase",campo,"FAS",10),"CODIGOACTIVIDAD"=>("gepro_actividad",campo,"ACT",10),"CODIGOTAREA"=>("gepro_tarea",campo,"TAR",10),"CODIGOENTREGABLE"=>("gepro_entregable",campo,"ENT",10),"CODIGOCONTRATO"=>("pecon_contrato",campo,"CON",10),"CODIGOCATEGORIA"=>("fin_categoria_gasto",campo,"CAT",10),"CODIGOMOVIMIENTO"=>("fin_movimiento_proyecto",campo,"MOV",10),_=>(tabla,campo,campo.Length>=3?campo[..3]:campo,10)}; }
}
