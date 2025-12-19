using Microsoft.EntityFrameworkCore;
using SurveySystem.Domain;
using SurveySystem.Domain.Surveys;

namespace SurveySystem.Infrastructure.Data
{
    public class SurveyDbContext : DbContext
    {
        public SurveyDbContext(DbContextOptions<SurveyDbContext> options) : base(options) { }

        public DbSet<Survey> Surveys => Set<Survey>();
        public DbSet<Submission> Submissions => Set<Submission>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Survey>(b =>
            {
                b.HasKey(s => s.Id);

                // SurveyPeriod como owned
                b.OwnsOne(s => s.Period, pb =>
                {
                    pb.Property(p => p.StartDate).IsRequired();
                    pb.Property(p => p.EndDate).IsRequired();
                });

                // Questions como owned collection
                b.OwnsMany(s => s.Questions, qb =>
                {
                    qb.WithOwner().HasForeignKey("SurveyId");

                    // chave sombra para itens da coleção
                    qb.Property<int>("Id");
                    qb.HasKey("Id");

                    qb.Property("Text").IsRequired();
                    qb.Property("Order").IsRequired();

                    // A propriedade Options é read-only e o EF não vai mapear por convenção corretamente
                    qb.Ignore("Options");

                    qb.OwnsMany(typeof(Option), "_options", ob =>
                    {
                        ob.WithOwner().HasForeignKey("QuestionId");
                        ob.Property<int>("Id");
                        ob.HasKey("Id");

                        ob.Property("Text").IsRequired();
                        ob.Property("Order").IsRequired();
                    });
                });
            });

            // Submission -> Answers (provável próximo erro se não mapear)
            modelBuilder.Entity<Submission>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.SurveyId).IsRequired();
                b.Property(x => x.SubmittedAt).IsRequired();

                b.OwnsMany(x => x.Answers, ab =>
                {
                    ab.WithOwner().HasForeignKey("SubmissionId");
                    ab.Property<int>("Id");
                    ab.HasKey("Id");

                    ab.Property(a => a.QuestionText).IsRequired();
                    ab.Property(a => a.OptionText).IsRequired();
                });
            });
        }
    }
}
