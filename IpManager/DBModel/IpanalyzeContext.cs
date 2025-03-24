using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace IpManager.DBModel;

public partial class IpanalyzeContext : DbContext
{
    public IpanalyzeContext()
    {
    }

    public IpanalyzeContext(DbContextOptions<IpanalyzeContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AnalyzeTb> AnalyzeTbs { get; set; }

    public virtual DbSet<CityTb> CityTbs { get; set; }

    public virtual DbSet<CountryTb> CountryTbs { get; set; }

    public virtual DbSet<LoginTb> LoginTbs { get; set; }

    public virtual DbSet<PcroomTb> PcroomTbs { get; set; }

    public virtual DbSet<PinglogTb> PinglogTbs { get; set; }

    public virtual DbSet<TimeTb> TimeTbs { get; set; }

    public virtual DbSet<TownTb> TownTbs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseMySql("server=127.0.0.1;port=3306;database=ipanalyze;user=root;password=rladyddn!!95", ServerVersion.Parse("11.4.5-mariadb"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("latin1_swedish_ci")
            .HasCharSet("latin1");

        modelBuilder.Entity<AnalyzeTb>(entity =>
        {
            entity.HasKey(e => e.Pid).HasName("PRIMARY");

            entity.ToTable("analyze_tb");

            entity.HasIndex(e => e.TopOpratePcroomtbId, "FK_OPRATE_PCROOMTB_ID_202503242053");

            entity.HasIndex(e => e.TopSalesPcroomtbId, "FK_SALES_PCROOMTB_ID_202503242053");

            entity.HasIndex(e => e.TowntbId, "FK_TOWNTB_ID_202503242052");

            entity.Property(e => e.Pid)
                .HasColumnType("int(11)")
                .HasColumnName("PID");
            entity.Property(e => e.CreateDt)
                .HasDefaultValueSql("current_timestamp()")
                .HasComment("생성일자")
                .HasColumnType("datetime")
                .HasColumnName("CREATE_DT");
            entity.Property(e => e.TopOpratePcroomtbId)
                .HasComment("가동률1위 매장 인덱스")
                .HasColumnType("int(11)")
                .HasColumnName("TOP_OPRATE_PCROOMTB_ID");
            entity.Property(e => e.TopSalesPcroomtbId)
                .HasComment("매출 1위 매장 인덱스")
                .HasColumnType("int(11)")
                .HasColumnName("TOP_SALES_PCROOMTB_ID");
            entity.Property(e => e.TowntbId)
                .HasComment("매출 1위 동네 인덱스")
                .HasColumnType("int(11)")
                .HasColumnName("TOWNTB_ID");

            entity.HasOne(d => d.TopOpratePcroomtb).WithMany(p => p.AnalyzeTbTopOpratePcroomtbs)
                .HasForeignKey(d => d.TopOpratePcroomtbId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OPRATE_PCROOMTB_ID_202503242053");

            entity.HasOne(d => d.TopSalesPcroomtb).WithMany(p => p.AnalyzeTbTopSalesPcroomtbs)
                .HasForeignKey(d => d.TopSalesPcroomtbId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SALES_PCROOMTB_ID_202503242053");

            entity.HasOne(d => d.Towntb).WithMany(p => p.AnalyzeTbs)
                .HasForeignKey(d => d.TowntbId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TOWNTB_ID_202503242052");
        });

        modelBuilder.Entity<CityTb>(entity =>
        {
            entity.HasKey(e => e.Pid).HasName("PRIMARY");

            entity
                .ToTable("city_tb", tb => tb.HasComment("(시/군/구) 정보"))
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_general_ci");

            entity.HasIndex(e => e.Name, "UK").IsUnique();

            entity.HasIndex(e => e.CountrytbId, "fk_countrytb202502191016");

            entity.Property(e => e.Pid)
                .HasColumnType("int(11)")
                .HasColumnName("PID");
            entity.Property(e => e.CountrytbId)
                .HasComment("(도/시) 테이블 키")
                .HasColumnType("int(11)")
                .HasColumnName("COUNTRYTB_ID");
            entity.Property(e => e.CreateDt)
                .HasDefaultValueSql("current_timestamp()")
                .HasComment("생성일")
                .HasColumnType("datetime")
                .HasColumnName("CREATE_DT");
            entity.Property(e => e.DelYn)
                .HasDefaultValueSql("'0'")
                .HasComment("삭제유무")
                .HasColumnName("DEL_YN");
            entity.Property(e => e.DeleteDt)
                .HasComment("삭제일")
                .HasColumnType("datetime")
                .HasColumnName("DELETE_DT");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasComment("(시/군/구) 명칭")
                .HasColumnName("NAME");
            entity.Property(e => e.UpdateDt)
                .HasComment("수정일")
                .HasColumnType("datetime")
                .HasColumnName("UPDATE_DT");

            entity.HasOne(d => d.Countrytb).WithMany(p => p.CityTbs)
                .HasForeignKey(d => d.CountrytbId)
                .HasConstraintName("fk_countrytb202502191016");
        });

        modelBuilder.Entity<CountryTb>(entity =>
        {
            entity.HasKey(e => e.Pid).HasName("PRIMARY");

            entity
                .ToTable("country_tb", tb => tb.HasComment("(도/시) 정보"))
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_general_ci");

            entity.HasIndex(e => e.Name, "UK").IsUnique();

            entity.Property(e => e.Pid)
                .HasColumnType("int(11)")
                .HasColumnName("PID");
            entity.Property(e => e.CreateDt)
                .HasDefaultValueSql("current_timestamp()")
                .HasComment("생성일")
                .HasColumnType("datetime")
                .HasColumnName("CREATE_DT");
            entity.Property(e => e.DelYn)
                .HasDefaultValueSql("'0'")
                .HasComment("삭제유무")
                .HasColumnName("DEL_YN");
            entity.Property(e => e.DeleteDt)
                .HasComment("삭제일")
                .HasColumnType("datetime")
                .HasColumnName("DELETE_DT");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasComment("(도/시)명칭")
                .HasColumnName("NAME");
            entity.Property(e => e.UpdateDt)
                .HasComment("수정일")
                .HasColumnType("datetime")
                .HasColumnName("UPDATE_DT");
        });

        modelBuilder.Entity<LoginTb>(entity =>
        {
            entity.HasKey(e => e.Pid).HasName("PRIMARY");

            entity
                .ToTable("login_tb")
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_general_ci");

            entity.HasIndex(e => e.CountryId, "FK_login_country");

            entity.HasIndex(e => e.Uid, "UK").IsUnique();

            entity.Property(e => e.Pid)
                .HasComment("PROCESS_ID")
                .HasColumnType("int(11)")
                .HasColumnName("PID");
            entity.Property(e => e.AdminYn)
                .HasComment("관리자계정 유무")
                .HasColumnName("ADMIN_YN");
            entity.Property(e => e.CountryId)
                .HasComment("일반사용자 (도/시)ID")
                .HasColumnType("int(11)")
                .HasColumnName("COUNTRY_ID");
            entity.Property(e => e.CreateDt)
                .HasDefaultValueSql("current_timestamp()")
                .HasComment("생성일")
                .HasColumnType("datetime")
                .HasColumnName("CREATE_DT");
            entity.Property(e => e.DelYn)
                .HasComment("삭제여부")
                .HasColumnName("DEL_YN");
            entity.Property(e => e.DeleteDt)
                .HasComment("삭제일")
                .HasColumnType("datetime")
                .HasColumnName("DELETE_DT");
            entity.Property(e => e.MasterYn)
                .HasComment("마스터계정 유무")
                .HasColumnName("MASTER_YN");
            entity.Property(e => e.Pwd)
                .HasMaxLength(25)
                .HasComment("비밀번호")
                .HasColumnName("PWD");
            entity.Property(e => e.Uid)
                .HasMaxLength(25)
                .HasComment("사용자ID")
                .HasColumnName("UID");
            entity.Property(e => e.UpdateDt)
                .HasComment("수정일")
                .HasColumnType("datetime")
                .HasColumnName("UPDATE_DT");
            entity.Property(e => e.UseYn)
                .HasComment("로그인 승인 유무")
                .HasColumnName("USE_YN");

            entity.HasOne(d => d.Country).WithMany(p => p.LoginTbs)
                .HasForeignKey(d => d.CountryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_login_country");
        });

        modelBuilder.Entity<PcroomTb>(entity =>
        {
            entity.HasKey(e => e.Pid).HasName("PRIMARY");

            entity
                .ToTable("pcroom_tb")
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_general_ci");

            entity.HasIndex(e => new { e.Ip, e.Name, e.Port, e.Addr }, "UK").IsUnique();

            entity.HasIndex(e => e.CitytbId, "fk_pcroom_city");

            entity.HasIndex(e => e.CountrytbId, "fk_pcroom_country");

            entity.HasIndex(e => e.TowntbId, "fk_pcroom_town");

            entity.Property(e => e.Pid)
                .HasColumnType("int(11)")
                .HasColumnName("PID");
            entity.Property(e => e.Addr)
                .HasMaxLength(100)
                .HasComment("주소")
                .HasColumnName("ADDR");
            entity.Property(e => e.CitytbId)
                .HasComment("(시/군/구) 테이블 키")
                .HasColumnType("int(11)")
                .HasColumnName("CITYTB_ID");
            entity.Property(e => e.CountrytbId)
                .HasComment("(도/시) 테이블 키")
                .HasColumnType("int(11)")
                .HasColumnName("COUNTRYTB_ID");
            entity.Property(e => e.CreateDt)
                .HasDefaultValueSql("current_timestamp()")
                .HasComment("생성일")
                .HasColumnType("datetime")
                .HasColumnName("CREATE_DT");
            entity.Property(e => e.DelYn)
                .HasDefaultValueSql("'0'")
                .HasComment("삭제유무")
                .HasColumnName("DEL_YN");
            entity.Property(e => e.DeleteDt)
                .HasComment("삭제일")
                .HasColumnType("datetime")
                .HasColumnName("DELETE_DT");
            entity.Property(e => e.Ip)
                .HasMaxLength(25)
                .HasComment("아이피 주소")
                .HasColumnName("IP");
            entity.Property(e => e.Memo)
                .HasMaxLength(255)
                .HasComment("메모")
                .HasColumnName("MEMO");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasComment("피시방 상호")
                .HasColumnName("NAME");
            entity.Property(e => e.PcSpec)
                .HasMaxLength(255)
                .HasComment("PC 사양")
                .HasColumnName("PC_SPEC");
            entity.Property(e => e.Port)
                .HasComment("포트번호")
                .HasColumnType("int(11)")
                .HasColumnName("PORT");
            entity.Property(e => e.Price)
                .HasComment("요금제 가격")
                .HasColumnName("PRICE");
            entity.Property(e => e.PricePercent)
                .HasComment("PC 요금제 비율")
                .HasColumnName("PRICE_PERCENT");
            entity.Property(e => e.Seatnumber)
                .HasComment("좌석수")
                .HasColumnType("int(11)")
                .HasColumnName("SEATNUMBER");
            entity.Property(e => e.Telecom)
                .HasMaxLength(30)
                .HasComment("통신사")
                .HasColumnName("TELECOM");
            entity.Property(e => e.TowntbId)
                .HasComment("(읍/면/동) 테이블 키")
                .HasColumnType("int(11)")
                .HasColumnName("TOWNTB_ID");
            entity.Property(e => e.UpdateDt)
                .HasComment("수정일")
                .HasColumnType("datetime")
                .HasColumnName("UPDATE_DT");

            entity.HasOne(d => d.Citytb).WithMany(p => p.PcroomTbs)
                .HasForeignKey(d => d.CitytbId)
                .HasConstraintName("fk_pcroom_city");

            entity.HasOne(d => d.Countrytb).WithMany(p => p.PcroomTbs)
                .HasForeignKey(d => d.CountrytbId)
                .HasConstraintName("fk_pcroom_country");

            entity.HasOne(d => d.Towntb).WithMany(p => p.PcroomTbs)
                .HasForeignKey(d => d.TowntbId)
                .HasConstraintName("fk_pcroom_town");
        });

        modelBuilder.Entity<PinglogTb>(entity =>
        {
            entity.HasKey(e => e.Pid).HasName("PRIMARY");

            entity
                .ToTable("pinglog_tb", tb => tb.HasComment("핑 정보"))
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_general_ci");

            entity.HasIndex(e => e.PcroomtbId, "fk_PINGLOG_pcroom202502191019");

            entity.HasIndex(e => e.CitytbId, "fk_PLINGLOG_city202502191020");

            entity.HasIndex(e => e.CountrytbId, "fk_PLINGLOG_country202502191020");

            entity.HasIndex(e => e.TimetbId, "fk_PLINGLOG_time202502191020");

            entity.HasIndex(e => e.TowntbId, "fk_PLINGLOG_town202502191020");

            entity.Property(e => e.Pid)
                .HasColumnType("int(11)")
                .HasColumnName("PID");
            entity.Property(e => e.CitytbId)
                .HasComment("(시/군/구) 테이블 키")
                .HasColumnType("int(11)")
                .HasColumnName("CITYTB_ID");
            entity.Property(e => e.CountrytbId)
                .HasComment("(도/시) 테이블 키")
                .HasColumnType("int(11)")
                .HasColumnName("COUNTRYTB_ID");
            entity.Property(e => e.CreateDt)
                .HasDefaultValueSql("current_timestamp()")
                .HasComment("생성일")
                .HasColumnType("datetime")
                .HasColumnName("CREATE_DT");
            entity.Property(e => e.DelYn)
                .HasDefaultValueSql("'0'")
                .HasComment("삭제유무")
                .HasColumnName("DEL_YN");
            entity.Property(e => e.DeleteDt)
                .HasComment("삭제일")
                .HasColumnType("datetime")
                .HasColumnName("DELETE_DT");
            entity.Property(e => e.PcCount)
                .HasComment("총 PC수")
                .HasColumnType("int(11)")
                .HasColumnName("PC_COUNT");
            entity.Property(e => e.PcRate)
                .HasComment("가동률")
                .HasColumnName("PC_RATE");
            entity.Property(e => e.PcroomtbId)
                .HasComment("PC방 테이블 키")
                .HasColumnType("int(11)")
                .HasColumnName("PCROOMTB_ID");
            entity.Property(e => e.Price)
                .HasComment("총금액")
                .HasColumnName("PRICE");
            entity.Property(e => e.TimetbId)
                .HasComment("시간 테이블 키")
                .HasColumnType("int(11)")
                .HasColumnName("TIMETB_ID");
            entity.Property(e => e.TowntbId)
                .HasComment("(읍/면/동) 테이블 키")
                .HasColumnType("int(11)")
                .HasColumnName("TOWNTB_ID");
            entity.Property(e => e.UpdateDt)
                .HasComment("수정일")
                .HasColumnType("datetime")
                .HasColumnName("UPDATE_DT");
            entity.Property(e => e.UsedPc)
                .HasComment("사용대수")
                .HasColumnType("int(11)")
                .HasColumnName("USED_PC");

            entity.HasOne(d => d.Citytb).WithMany(p => p.PinglogTbs)
                .HasForeignKey(d => d.CitytbId)
                .HasConstraintName("fk_PLINGLOG_city202502191020");

            entity.HasOne(d => d.Countrytb).WithMany(p => p.PinglogTbs)
                .HasForeignKey(d => d.CountrytbId)
                .HasConstraintName("fk_PLINGLOG_country202502191020");

            entity.HasOne(d => d.Pcroomtb).WithMany(p => p.PinglogTbs)
                .HasForeignKey(d => d.PcroomtbId)
                .HasConstraintName("fk_PINGLOG_pcroom202502191019");

            entity.HasOne(d => d.Timetb).WithMany(p => p.PinglogTbs)
                .HasForeignKey(d => d.TimetbId)
                .HasConstraintName("fk_PLINGLOG_time202502191020");

            entity.HasOne(d => d.Towntb).WithMany(p => p.PinglogTbs)
                .HasForeignKey(d => d.TowntbId)
                .HasConstraintName("fk_PLINGLOG_town202502191020");
        });

        modelBuilder.Entity<TimeTb>(entity =>
        {
            entity.HasKey(e => e.Pid).HasName("PRIMARY");

            entity
                .ToTable("time_tb")
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_general_ci");

            entity.Property(e => e.Pid)
                .HasColumnType("int(11)")
                .HasColumnName("PID");
            entity.Property(e => e.Time)
                .HasComment("00:00:00 ~ 24:00:00 / 30분단위")
                .HasColumnType("time")
                .HasColumnName("TIME");
        });

        modelBuilder.Entity<TownTb>(entity =>
        {
            entity.HasKey(e => e.Pid).HasName("PRIMARY");

            entity
                .ToTable("town_tb")
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_general_ci");

            entity.HasIndex(e => e.Name, "UK").IsUnique();

            entity.HasIndex(e => e.CitytbId, "fk_city202502192215");

            entity.HasIndex(e => e.CountytbId, "fk_country202502192215");

            entity.Property(e => e.Pid)
                .HasColumnType("int(11)")
                .HasColumnName("PID");
            entity.Property(e => e.CitytbId)
                .HasComment("(시/군/구) 테이블 키")
                .HasColumnType("int(11)")
                .HasColumnName("CITYTB_ID");
            entity.Property(e => e.CountytbId)
                .HasComment("(도/시) 테이블 키")
                .HasColumnType("int(11)")
                .HasColumnName("COUNTYTB_ID");
            entity.Property(e => e.CreateDt)
                .HasDefaultValueSql("current_timestamp()")
                .HasComment("생성일")
                .HasColumnType("datetime")
                .HasColumnName("CREATE_DT");
            entity.Property(e => e.DelYn)
                .HasDefaultValueSql("'0'")
                .HasComment("삭제유무")
                .HasColumnName("DEL_YN");
            entity.Property(e => e.DeleteDt)
                .HasComment("삭제일")
                .HasColumnType("datetime")
                .HasColumnName("DELETE_DT");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasComment("(읍/면/동) 명칭")
                .HasColumnName("NAME");
            entity.Property(e => e.UpdateDt)
                .HasComment("수정일")
                .HasColumnType("datetime")
                .HasColumnName("UPDATE_DT");

            entity.HasOne(d => d.Citytb).WithMany(p => p.TownTbs)
                .HasForeignKey(d => d.CitytbId)
                .HasConstraintName("fk_city202502192215");

            entity.HasOne(d => d.Countytb).WithMany(p => p.TownTbs)
                .HasForeignKey(d => d.CountytbId)
                .HasConstraintName("fk_country202502192215");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
