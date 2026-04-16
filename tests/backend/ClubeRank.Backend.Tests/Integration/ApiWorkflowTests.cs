using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ClubeRank.Domain.ValueObjects;

namespace ClubeRank.Backend.Tests.Integration;

public class ApiWorkflowTests : IClassFixture<ApiProcessFixture>
{
    private readonly HttpClient _client;

    public ApiWorkflowTests(ApiProcessFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task FluxoPrincipal_DeveExecutarDoInicioAoFim()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var login = await CreateOrganizationAndLoginAsync(suffix);

        var atletaAId = await CreateAthleteAsync(login, suffix, "Carlos", "Silva", Categoria.A);
        var atletaBId = await CreateAthleteAsync(login, suffix, "Bruno", "Souza", Categoria.A);
        var torneioId = await CreateTournamentAsync(login, suffix, Categoria.A);

        await AddAthleteToTournamentAsync(login, torneioId, atletaAId);
        await AddAthleteToTournamentAsync(login, torneioId, atletaBId);

        var confrontosResponse = await PostAuthorizedAsync(login, $"/api/torneios/{torneioId}/confrontos/gerar", new { });
        Assert.Equal(HttpStatusCode.OK, confrontosResponse.StatusCode);

        var confrontos = await confrontosResponse.Content.ReadFromJsonAsync<List<ConfrontoResponse>>();
        var confronto = Assert.Single(confrontos!);

        var vencedorEsperadoId = confronto.AtletaAId;
        var perdedorEsperadoId = confronto.AtletaBId;

        var resultadoResponse = await PostAuthorizedAsync(login, "/api/resultados", new
        {
            confrontoId = confronto.Id,
            tipoResultado = TipoResultado.VitoriaAtletaA.ToString(),
            sets = new[]
            {
                new { numero = 1, gamesAtletaA = 6, gamesAtletaB = 4, tieBreak = false },
                new { numero = 2, gamesAtletaA = 3, gamesAtletaB = 6, tieBreak = false },
                new { numero = 3, gamesAtletaA = 10, gamesAtletaB = 8, tieBreak = true }
            },
            justificativaWO = (string?)null
        });

        Assert.Equal(HttpStatusCode.NoContent, resultadoResponse.StatusCode);

        var rankingRequest = new HttpRequestMessage(HttpMethod.Get, "/api/rankings");
        rankingRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);
        rankingRequest.Headers.Add("X-Tenant-Id", login.TenantId.ToString());
        var rankingResponse = await _client.SendAsync(rankingRequest);

        Assert.Equal(HttpStatusCode.OK, rankingResponse.StatusCode);
        var ranking = await rankingResponse.Content.ReadFromJsonAsync<List<AtletaResponse>>();
        Assert.NotNull(ranking);
        Assert.Equal(2, ranking!.Count);
        Assert.Equal(1014, ranking.Single(x => x.Id == vencedorEsperadoId).PontuacaoAtual);
        Assert.Equal(997, ranking.Single(x => x.Id == perdedorEsperadoId).PontuacaoAtual);
    }

    [Fact]
    public async Task RegistrarResultadoWo_SemJustificativa_DeveRetornarBadRequest()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var login = await CreateOrganizationAndLoginAsync(suffix);

        var atletaAId = await CreateAthleteAsync(login, suffix, "Alice", "Lima", Categoria.B);
        var atletaBId = await CreateAthleteAsync(login, suffix, "Bianca", "Moraes", Categoria.B);
        var torneioId = await CreateTournamentAsync(login, suffix, Categoria.B);

        await AddAthleteToTournamentAsync(login, torneioId, atletaAId);
        await AddAthleteToTournamentAsync(login, torneioId, atletaBId);

        var confrontosResponse = await PostAuthorizedAsync(login, $"/api/torneios/{torneioId}/confrontos/gerar", new { });
        var confrontos = await confrontosResponse.Content.ReadFromJsonAsync<List<ConfrontoResponse>>();
        var confronto = Assert.Single(confrontos!);

        var response = await PostAuthorizedAsync(login, "/api/resultados", new
        {
            confrontoId = confronto.Id,
            tipoResultado = TipoResultado.WO.ToString(),
            justificativaWO = (string?)null
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CriarAtleta_ComEmailDuplicadoNaMesmaOrganizacao_DeveRetornarBadRequest()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var login = await CreateOrganizationAndLoginAsync(suffix);
        var email = $"duplicado.{suffix}@cluberank.test";

        var firstResponse = await PostAuthorizedAsync(login, "/api/atletas", new
        {
            primeiroNome = "Rafa",
            sobrenome = "Dias",
            genero = Genero.Masculino.ToString(),
            email,
            telefone = "11999998888",
            categoria = Categoria.C.ToString()
        });

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        var duplicateResponse = await PostAuthorizedAsync(login, "/api/atletas", new
        {
            primeiroNome = "Rafa",
            sobrenome = "Dias 2",
            genero = Genero.Masculino.ToString(),
            email,
            telefone = "11999997777",
            categoria = Categoria.C.ToString()
        });

        Assert.Equal(HttpStatusCode.BadRequest, duplicateResponse.StatusCode);
    }

    [Fact]
    public async Task ListarAtletas_DeveIsolarDadosPorTenant()
    {
        var loginA = await CreateOrganizationAndLoginAsync($"a{Guid.NewGuid():N}"[..9]);
        var loginB = await CreateOrganizationAndLoginAsync($"b{Guid.NewGuid():N}"[..9]);

        await CreateAthleteAsync(loginA, "tenantA", "Carlos", "TenantA", Categoria.A);
        await CreateAthleteAsync(loginB, "tenantB", "Bruno", "TenantB", Categoria.A);

        var requestA = new HttpRequestMessage(HttpMethod.Get, "/api/atletas?pageNumber=1&pageSize=50");
        requestA.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginA.AccessToken);
        requestA.Headers.Add("X-Tenant-Id", loginA.TenantId.ToString());
        var responseA = await _client.SendAsync(requestA);
        var atletasA = await responseA.Content.ReadFromJsonAsync<List<AtletaResponse>>();

        var requestB = new HttpRequestMessage(HttpMethod.Get, "/api/atletas?pageNumber=1&pageSize=50");
        requestB.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginB.AccessToken);
        requestB.Headers.Add("X-Tenant-Id", loginB.TenantId.ToString());
        var responseB = await _client.SendAsync(requestB);
        var atletasB = await responseB.Content.ReadFromJsonAsync<List<AtletaResponse>>();

        Assert.All(atletasA!, atleta => Assert.Equal(loginA.TenantId, atleta.OrganizacaoId));
        Assert.All(atletasB!, atleta => Assert.Equal(loginB.TenantId, atleta.OrganizacaoId));
        Assert.DoesNotContain(atletasA!, atleta => atletasB!.Any(other => other.Id == atleta.Id));
    }

    private async Task<LoginResponse> CreateOrganizationAndLoginAsync(string suffix)
    {
        var email = $"admin.{suffix}@cluberank.test";
        var password = "SenhaFase1!";

        var organizationResponse = await _client.PostAsJsonAsync("/api/organizacoes", new
        {
            nome = $"Clube {suffix}",
            email,
            telefone = "11999990000",
            modalidade = "Tenis",
            plano = TipoPlano.Basico.ToString(),
            nomeAdministrador = $"Admin {suffix}",
            senhaAdministrador = password
        });

        Assert.True(
            organizationResponse.StatusCode == HttpStatusCode.Created,
            $"Falha ao criar organizacao: {(int)organizationResponse.StatusCode} - {await organizationResponse.Content.ReadAsStringAsync()}");

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/token", new
        {
            email,
            password
        });

        Assert.True(
            loginResponse.StatusCode == HttpStatusCode.OK,
            $"Falha no login: {(int)loginResponse.StatusCode} - {await loginResponse.Content.ReadAsStringAsync()}");
        var json = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync()).RootElement;
        var login = new LoginResponse(
            json.GetProperty("accessToken").GetString() ?? string.Empty,
            json.GetProperty("tenantId").GetGuid(),
            json.GetProperty("roles").EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToArray());

        Assert.False(string.IsNullOrWhiteSpace(login.AccessToken), "O login retornou token vazio.");
        return login;
    }

    private async Task<Guid> CreateAthleteAsync(LoginResponse login, string suffix, string firstName, string lastName, Categoria categoria)
    {
        var response = await PostAuthorizedAsync(login, "/api/atletas", new
        {
            primeiroNome = firstName,
            sobrenome = lastName,
            genero = Genero.Masculino.ToString(),
            email = $"{firstName.ToLowerInvariant()}.{suffix}@cluberank.test",
            telefone = "11999991111",
            categoria = categoria.ToString()
        });

        Assert.True(
            response.StatusCode == HttpStatusCode.Created,
            $"Falha ao criar atleta: {(int)response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
        var payload = await response.Content.ReadFromJsonAsync<CreatedIdResponse>();
        return payload!.Id;
    }

    private async Task<Guid> CreateTournamentAsync(LoginResponse login, string suffix, Categoria categoria)
    {
        var response = await PostAuthorizedAsync(login, "/api/torneios", new
        {
            nome = $"Torneio {suffix}",
            descricao = "Fluxo principal do MVP",
            tipo = TipoTorneio.Ladder.ToString(),
            categoria = categoria.ToString(),
            dataInicio = DateTime.UtcNow,
            dataFim = DateTime.UtcNow.AddDays(7),
            pontuacaoVitoria = 10,
            pontuacaoDerrota = -5,
            pontuacaoEmpate = 0,
            pontuacaoWO = -20,
            pontuacaoSetVencido = 2,
            melhorDeSets = 3,
            permiteEmpate = true
        });

        Assert.True(
            response.StatusCode == HttpStatusCode.Created,
            $"Falha ao criar torneio: {(int)response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
        var payload = await response.Content.ReadFromJsonAsync<CreatedIdResponse>();
        return payload!.Id;
    }

    private async Task AddAthleteToTournamentAsync(LoginResponse login, Guid tournamentId, Guid athleteId)
    {
        var response = await PostAuthorizedAsync(login, $"/api/torneios/{tournamentId}/atletas", new { atletaId = athleteId });

        Assert.True(
            response.StatusCode == HttpStatusCode.NoContent,
            $"Falha ao associar atleta: {(int)response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
    }

    private Task<HttpResponseMessage> PostAuthorizedAsync(LoginResponse login, string url, object payload)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);
        request.Headers.Add("X-Tenant-Id", login.TenantId.ToString());
        return _client.SendAsync(request);
    }

    private sealed record LoginResponse(string AccessToken, Guid TenantId, string[] Roles);
    private sealed record CreatedIdResponse(Guid Id);
    private sealed record AtletaResponse(Guid Id, Guid OrganizacaoId, int PontuacaoAtual);
    private sealed record ConfrontoResponse(Guid Id, Guid AtletaAId, Guid AtletaBId);
}
