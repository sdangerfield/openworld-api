//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18444
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Collections.Generic;
using FluentNHibernate.Mapping;
using FluentNHibernate.Automapping;
using FluentNHibernate.MappingModel;
using FluentNHibernate.Conventions.Helpers;
using FluentNHibernate.Conventions.AcceptanceCriteria;
using NHibernate;
using NHibernate.Criterion;
using FluentNHibernate;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using System.Diagnostics;
using NHibernate.Tool.hbm2ddl;
using norwold.model;
using DataModel.libHosting;


namespace  DataModel.libDB
{

    
	public class nwdbConnection
	{
        //Sets whether to use the Azure database or local
        private bool bCloud = false;

        
        private ISessionFactory factory;
        //private FluentConfiguration myConfig;
        private dbConnDetails details;

        protected class dbConnDetails {
            public string db;
            public string uname;
            public string pw;
            public string svr;

            public dbConnDetails(string database,string username,string password, string server) {
                this.db = database;
                this.uname = username;
                this.pw = password;
                this.svr = server;
            }
        }


        private String strCommand;
        private String strConnection;
        private SqlConnection myConn;
		//Constructor
		public nwdbConnection() {
            
            InitDBConn();
            this.factory = CreateSessionFactory(false, details.db,details.uname,details.pw);
           
		}

        public void InitDBConn() {
            if (this.bCloud) {
                details = new dbConnDetails("openworld","nwlogin","However182$","o714tvghlu.database.windows.net");
            } else {
                details = new dbConnDetails("openworld","nwlogin","nwlogin","STEVE-PC\\SQLEXPRESS");
            }
        }

        public nwdbConnection(bool DropCreate)
        {
            InitDBConn();
            
            this.factory = CreateSessionFactory(DropCreate, details.db, details.uname, details.pw);

        }

        public nwdbConnection(bool DropCreate, string TempDB,string uname,string pw)
        {
            this.factory = CreateSessionFactory(DropCreate,TempDB,uname,pw);

        }

        public nwdbConnection(bool DropCreate,bool CreateDB,string DBName)
        {
            if (CreateDB == true)
            {
                if (DBExists(DBName) == false)
                {
                    CreateDatabase(DBName);
                }

            }
            
            //this.factory = CreateSessionFactory(true, DBName, uname, pw);

        }

        // properties
		public ISessionFactory Factory 
        {
			get { return this.factory; }
		}

        private ISessionFactory CreateSessionFactory(bool DropCreate,string Database,string user,string password)
		{
            
        //var cfg = new nwAutoMapConfiguration();
            if (DropCreate == false)
            {
                return Fluently.Configure()
                    .Database(
                        MsSqlConfiguration.MsSql2008.ShowSql().ConnectionString(
                            c => c
                            .Server(details.svr)
                            .Database(Database)
                            .Username(user)
                            .Password(password)
                            )
                        )

                     //fluent mapped 
                        
                      //  when Bugged use this to get nhibernate error
                     .Mappings(m =>
                     {
                         var persistenceModel = new PersistenceModel();
                         persistenceModel.AddMappingsFromAssembly(typeof(hostChar).Assembly);
                         persistenceModel.ValidationEnabled = false; // this makes the trick
                         m.UsePersistenceModel(persistenceModel);
                     })
                    
                     //.Mappings(m => m.FluentMappings.AddFromAssemblyOf<hostChar>())
                     //UpdateSchema
                     .ExposeConfiguration(cfg => new SchemaUpdate(cfg).Execute(true, true))
                     .BuildSessionFactory();
           
            
            
            }
            else
            {
                if (this.DBExists(Database))
                {
                    DropDatabase(Database);
                    CreateDatabase(Database);
                }
                return Fluently.Configure()
                        .Database(
                            MsSqlConfiguration.MsSql2012.ShowSql().ConnectionString(
                                c => c
                                .Server(details.svr)
                                .Database(Database)
                                .Username(user)
                                .Password(password)
                                )
                            )
                         //fluent mapped
                         .Mappings(m => m.FluentMappings.AddFromAssemblyOf<hostChar>())
                         .ExposeConfiguration(cfg => new SchemaExport(cfg).Create(true,true))
                         .BuildSessionFactory();
                //automapped
                //.Mappings(m => m.AutoMappings.Add(AutoMap.AssemblyOf<ConstSysDmgType>(cfg)))
            }
		}

        public string GetTableName(object o)
        {

            var u = this.factory.GetClassMetadata(o.GetType()) as NHibernate.Persister.Entity.AbstractEntityPersister;
            return u.TableName;
        }

        protected string BuildConnectionString(string server, bool iSecurity, string username, string password, string iCatalog)
        {
            String build;
            build = "Server=" + server + ";";

            if (iSecurity)
            {
                build = build + "Integrated security=true;";
            }
            else
            {
                build = build + "Integrated security=false;";
                build = build + "User ID=" + username + ";";
                build = build + "Password=" + password + ";";
            }
            
            if ( iCatalog == null || iCatalog == "") {
                return build;
            } else {
                return build + "database=\"" + iCatalog + "\";";
            }
                                   
        }

        protected String BuildDefaultConnection()
        {
            return BuildConnectionString(details.svr, false, details.uname, details.pw, details.db);
        }
        protected String BuildMasterConnection()
        {
            return BuildConnectionString(details.svr, false, details.uname, details.pw, "master");
        }

        public bool DropDatabase(string dbName)
        {
            SqlCommand myCommand;
            bool r;

            r = false;
            strConnection = BuildMasterConnection();
            myConn = new SqlConnection(strConnection);

            strCommand = "DROP DATABASE \"" + dbName + "\"";
            myCommand = new SqlCommand(strCommand, myConn);


            try
            {
                myConn.Open();
                myCommand.ExecuteNonQuery();
                //set DBOwner
                //myConn.ChangeDatabase(dbName);
                //myCommand2.ExecuteNonQuery();
                r = true;
            }
            catch (System.Exception ex)
            {
                Debug.Print("Fucked" + ex.ToString());
            }
            finally
            {
                if (myConn.State == ConnectionState.Open)
                {
                    myConn.Close();
                }
            }
            return r;

        }

        public bool CreateDatabase(string dbName)
        {
            //creates the database
            //uses the initialized default values
            //assumes Server authentiction has createDB rights
            //must connect to master to create db;
            
            SqlCommand myCommand;
            bool r;

            r = false;
            strConnection = BuildMasterConnection();
            myConn = new SqlConnection(strConnection);

            strCommand = "CREATE DATABASE '"+ dbName+"'";
            myCommand = new SqlCommand(strCommand, myConn);
            

            try
            {
                myConn.Open();
                myCommand.ExecuteNonQuery();
                //set DBOwner
                //myConn.ChangeDatabase(dbName);
                //myCommand2.ExecuteNonQuery();
                r = true;
             }
            catch (System.Exception ex)
            {
                Debug.Print("Fucked"+ex.ToString());
            }
            finally
            {
                if (myConn.State == ConnectionState.Open)
                {
                    myConn.Close();
                }
            }
            return r;
        }

        public virtual bool DBExists(string DBName)
        {
            
            myConn = new SqlConnection(BuildMasterConnection());
            strCommand = "SELECT * FROM master.sys.databases where name ='" + DBName + "'";

            try
            {
                using (myConn)
                {
                    myConn.Open();
                    myConn.ChangeDatabase("master");

                    using (SqlCommand sqlCmd = new SqlCommand(strCommand, myConn))
                    {
                        int exists = sqlCmd.ExecuteNonQuery();

                        if (exists <= 0)
                            return false;
                        else
                            return true;
                    }
                }
            }
            finally
            {
                if (myConn.State == ConnectionState.Open)
                {
                    myConn.Close();
                }
            }
        }

public class SqlStatementInterceptor : EmptyInterceptor
{
    public override NHibernate.SqlCommand.SqlString OnPrepareStatement(NHibernate.SqlCommand.SqlString sql)
    {
        Trace.WriteLine(sql.ToString());
        return sql;
    }
}
	}
}

