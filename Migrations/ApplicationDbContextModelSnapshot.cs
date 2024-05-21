﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TelegramBudget.Data;

#nullable disable

namespace TelegramBudget.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("TelegramBudget.Data.Entities.Budget", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<long?>("CreatedBy")
                        .HasColumnType("bigint");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("character varying(250)");

                    b.HasKey("Id");

                    b.HasIndex("CreatedBy");

                    b.HasIndex("Name", "CreatedBy")
                        .IsUnique();

                    b.ToTable("Budget", (string)null);
                });

            modelBuilder.Entity("TelegramBudget.Data.Entities.Participant", b =>
                {
                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.Property<Guid>("BudgetId")
                        .HasColumnType("uuid");

                    b.HasKey("UserId", "BudgetId");

                    b.HasIndex("BudgetId");

                    b.ToTable("Participant", (string)null);
                });

            modelBuilder.Entity("TelegramBudget.Data.Entities.Transaction", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric");

                    b.Property<Guid>("BudgetId")
                        .HasColumnType("uuid");

                    b.Property<string>("Comment")
                        .HasMaxLength(250)
                        .HasColumnType("character varying(250)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long?>("CreatedBy")
                        .HasColumnType("bigint");

                    b.Property<int>("MessageId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("BudgetId");

                    b.HasIndex("CreatedAt");

                    b.HasIndex("CreatedBy");

                    b.HasIndex("CreatedBy", "MessageId")
                        .IsUnique();

                    b.ToTable("Transaction", (string)null);
                });

            modelBuilder.Entity("TelegramBudget.Data.Entities.Transaction+Version", b =>
                {
                    b.Property<int>("Number")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Number"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long?>("CreatedBy")
                        .HasColumnType("bigint");

                    b.Property<Guid>("EntityId")
                        .HasColumnType("uuid");

                    b.Property<string>("Serialized")
                        .HasColumnType("text");

                    b.HasKey("Number");

                    b.HasIndex("CreatedAt");

                    b.HasIndex("CreatedBy");

                    b.HasIndex("EntityId");

                    b.ToTable("Transaction_Versions", (string)null);
                });

            modelBuilder.Entity("TelegramBudget.Data.Entities.TransactionConfirmation", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("MessageId")
                        .HasColumnType("integer");

                    b.Property<long>("RecipientId")
                        .HasColumnType("bigint");

                    b.Property<Guid>("TransactionId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("CreatedAt");

                    b.HasIndex("RecipientId");

                    b.HasIndex("TransactionId");

                    b.HasIndex("MessageId", "RecipientId")
                        .IsUnique();

                    b.ToTable("TransactionConfirmation", (string)null);
                });

            modelBuilder.Entity("TelegramBudget.Data.Entities.User", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint");

                    b.Property<Guid?>("ActiveBudgetId")
                        .HasColumnType("uuid");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("LastName")
                        .HasColumnType("text");

                    b.Property<int?>("PromptMessageId")
                        .HasColumnType("integer");

                    b.Property<int?>("PromptSubject")
                        .HasColumnType("integer");

                    b.Property<TimeSpan>("TimeZone")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("interval")
                        .HasDefaultValue(new TimeSpan(0, 0, 0, 0, 0));

                    b.HasKey("Id");

                    b.HasIndex("ActiveBudgetId");

                    b.ToTable("User", (string)null);
                });

            modelBuilder.Entity("TelegramBudget.Data.Entities.Budget", b =>
                {
                    b.HasOne("TelegramBudget.Data.Entities.User", "Owner")
                        .WithMany("OwnedBudgets")
                        .HasForeignKey("CreatedBy")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("TelegramBudget.Data.Entities.Participant", b =>
                {
                    b.HasOne("TelegramBudget.Data.Entities.Budget", "Budget")
                        .WithMany("Participating")
                        .HasForeignKey("BudgetId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TelegramBudget.Data.Entities.User", "User")
                        .WithMany("Participating")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Budget");

                    b.Navigation("User");
                });

            modelBuilder.Entity("TelegramBudget.Data.Entities.Transaction", b =>
                {
                    b.HasOne("TelegramBudget.Data.Entities.Budget", "Budget")
                        .WithMany("Transactions")
                        .HasForeignKey("BudgetId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TelegramBudget.Data.Entities.User", "Author")
                        .WithMany("Transactions")
                        .HasForeignKey("CreatedBy")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Author");

                    b.Navigation("Budget");
                });

            modelBuilder.Entity("TelegramBudget.Data.Entities.TransactionConfirmation", b =>
                {
                    b.HasOne("TelegramBudget.Data.Entities.User", "Recipient")
                        .WithMany("TransactionConfirmations")
                        .HasForeignKey("RecipientId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TelegramBudget.Data.Entities.Transaction", "Transaction")
                        .WithMany("Confirmations")
                        .HasForeignKey("TransactionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Recipient");

                    b.Navigation("Transaction");
                });

            modelBuilder.Entity("TelegramBudget.Data.Entities.User", b =>
                {
                    b.HasOne("TelegramBudget.Data.Entities.Budget", "ActiveBudget")
                        .WithMany("ActiveUsers")
                        .HasForeignKey("ActiveBudgetId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("ActiveBudget");
                });

            modelBuilder.Entity("TelegramBudget.Data.Entities.Budget", b =>
                {
                    b.Navigation("ActiveUsers");

                    b.Navigation("Participating");

                    b.Navigation("Transactions");
                });

            modelBuilder.Entity("TelegramBudget.Data.Entities.Transaction", b =>
                {
                    b.Navigation("Confirmations");
                });

            modelBuilder.Entity("TelegramBudget.Data.Entities.User", b =>
                {
                    b.Navigation("OwnedBudgets");

                    b.Navigation("Participating");

                    b.Navigation("TransactionConfirmations");

                    b.Navigation("Transactions");
                });
#pragma warning restore 612, 618
        }
    }
}
