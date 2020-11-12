using Xunit;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using ApiAlunos.Services;
using ApiAlunos.Repositorio;
using ApiAlunos.Context;
using Microsoft.EntityFrameworkCore;
using ApiAlunos.Models;
using ApiAlunos.Controllers;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace ApiAlunos.Test
{
    public class AlunoControllerTest
    {
        private AlunoAppService _service;
        public static DbContextOptions<AlunoDbContext> dbContextOptions { get; }

        static AlunoControllerTest()
        {
            dbContextOptions = new DbContextOptionsBuilder<AlunoDbContext>()
                            .UseInMemoryDatabase("SQL")
                            .Options;
        }

        public AlunoControllerTest()
        {
            var context = new AlunoDbContext(dbContextOptions);

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.Alunos.AddRange(
                new Aluno() { Nome = "Lucas Panetto", Email = "lucaspanetto@ucl.br" },
                new Aluno() { Nome = "Leticia Ribeiro", Email = "ribeiro.leticia@uol.com" }
            );

            context.SaveChanges();

            AlunoRepository repository = new AlunoRepository(context);
            _service = new AlunoAppService(repository);
        }

        [Fact]
        public async void ObterAlunosSucesso()
        {
            var result = await _service.ObterTodosAlunos();

            Assert.IsType<List<Aluno>>(result);
            Assert.Equal(result.Count, 2);
        }


        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async void ObterAlunoPorIdSucesso(int valor)
        {
            var result = await _service.ObterAlunoPorId(valor);

            Assert.IsType<Aluno>(result);
            //diferente de nullo tbm, sua anta
        }

        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        public async void ObterAlunoPorIdErro(int valor)
        {
            var result = await _service.ObterAlunoPorId(valor);

            Assert.Equal(result, null);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async void AtualizarAlunoSucesso(int idAluno)
        {
            switch (idAluno)
            {
                case 1:
                    var aluno01 = await _service.ObterAlunoPorId(idAluno);
                    aluno01.Nome = "Teste01";
                    await _service.AtualizarAluno(aluno01);
                    var alunoPosAtualizacao01 = await _service.ObterAlunoPorId(idAluno);
                    Assert.Equal(alunoPosAtualizacao01, aluno01);
                    break;
                case 2:
                    var aluno02 = await _service.ObterAlunoPorId(idAluno);
                    aluno02.Nome = "Teste02";
                    await _service.AtualizarAluno(aluno02);
                    var alunoPosAtualizacao02 = await _service.ObterAlunoPorId(idAluno);
                    Assert.Equal(alunoPosAtualizacao02, aluno02);
                    break;
            }
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        public async void AtualizarAlunoErro(int idAluno)
        {
            switch (idAluno)
            {
                case 2:
                    try
                    {
                        var aluno01 = await _service.ObterAlunoPorId(idAluno);
                        aluno01.Nome = "";
                        await _service.AtualizarAluno(aluno01);
                    }
                    catch (System.Exception ex)
                    {
                        Assert.Equal(ex.Message, "Todos os campos s�o obrigat�rios!");
                    }
                    break;
                case 3:
                    var aluno02 = await _service.ObterAlunoPorId(idAluno);
                    Assert.Equal(null, aluno02);
                    break;
            }
        }

        [Fact]
        public void InserirAlunoSucesso()
        {
            Aluno aluno = new Aluno()
            {
                Nome = "C�sar Miranda",
                Email = "cesar.miranda@outlook.com"
            };

            var result = _service.CriarAluno(aluno).Result;

            Assert.Equal(result, aluno);
        }

        [Fact]
        public void InserirAlunoErro()
        {
            try
            {
                Aluno aluno = new Aluno();
                aluno.Nome = "";
                aluno.Email = "cesar.miranda@outlook.com";

                var result = _service.CriarAluno(aluno).Result;
            }
            catch (System.Exception ex)
            {
                Assert.Equal(ex.InnerException.Message, "Todos os campos s�o obrigat�rios!");
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void DeletarAlunoSucesso(int alunoId)
        {
            var alunoExcluido = _service.DeletarAluno(alunoId).Result;
            Assert.Equal(true, alunoExcluido);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-2)]
        public void DeletarAlunoErro(int alunoId)
        {
            try
            {
                var alunoExcluido = _service.DeletarAluno(alunoId).Result;
            }
            catch (System.Exception ex)
            {
                Assert.Equal(ex.InnerException.Message, "IdAluno para exclus�o inv�lido!");
            }
        }
    }
}