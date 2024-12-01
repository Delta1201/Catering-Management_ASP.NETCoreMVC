using Microsoft.EntityFrameworkCore.Migrations;

namespace CateringManagement.Data
{
    public static class ExtraMigration
    {
        public static void Steps(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"
                    CREATE TRIGGER SetFunctionTimestampOnUpdate
                    AFTER UPDATE ON Functions
                    BEGIN
                        UPDATE Functions
                        SET RowVersion = randomblob(8)
                        WHERE rowid = NEW.rowid;
                    END
                ");
            migrationBuilder.Sql(
                @"
                    CREATE TRIGGER SetFunctionTimestampOnInsert
                    AFTER INSERT ON Functions
                    BEGIN
                        UPDATE Functions
                        SET RowVersion = randomblob(8)
                        WHERE rowid = NEW.rowid;
                    END
                ");
            migrationBuilder.Sql(
                @"
                    Drop View IF EXISTS [FunctionRevenueSummary];
                    Create View FunctionRevenueSummary as
                    Select t.ID, t.Name, 
	                    ifnull(Count(f.ID),0) as TotalNumber,
	                    ifnull(Avg(f.PerPersonCharge),0) as AveragePPCharge,
	                    ifnull(Avg(f.GuaranteedNumber),0) as AverageGuarNo,
	                    ifnull(Sum(f.BaseCharge + f.SOCAN + (f.PerPersonCharge * f.GuaranteedNumber)),0) as TotalValue,
	                    ifnull(Avg(f.BaseCharge + f.SOCAN + (f.PerPersonCharge * f.GuaranteedNumber)),0) as AvgValue,
	                    ifnull(Max(f.BaseCharge + f.SOCAN + (f.PerPersonCharge * f.GuaranteedNumber)),0) as MaxValue
                    From FunctionTypes t left join Functions f
	                    on t.ID = f.FunctionTypeID
                    Group By t.ID, t.Name;
                ");
        }
    }
}
