import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatListModule } from '@angular/material/list';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { MatToolbarModule } from '@angular/material/toolbar';
import { environment } from '../environments/environment';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatChipsModule,
    MatDividerModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatListModule,
    MatProgressBarModule,
    MatSelectModule,
    MatSnackBarModule,
    MatTabsModule,
    MatToolbarModule
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly http = inject(HttpClient);
  private readonly snackBar = inject(MatSnackBar);
  private readonly apiUrl = environment.apiUrl.replace(/\/$/, '');

  protected readonly planos = ['Basico', 'Profissional', 'Empresarial'];
  protected readonly generos = ['Masculino', 'Feminino', 'Outro'];
  protected readonly categorias = ['A', 'B', 'C', 'Iniciante', 'Especial'];
  protected readonly tiposTorneio = ['Ladder', 'RoundRobin'];
  protected readonly statusAtleta = ['Ativo', 'Inativo', 'Suspenso'];
  protected readonly tiposResultado = ['VitoriaAtletaA', 'VitoriaAtletaB', 'Empate', 'WO'];
  protected readonly secoes: SectionKey[] = ['atletas', 'torneios', 'ranking', 'auditoria'];

  protected readonly onboardingForm = this.fb.nonNullable.group({
    nome: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    telefone: [''],
    modalidade: ['Tenis', Validators.required],
    plano: ['Basico', Validators.required],
    nomeAdministrador: ['', Validators.required],
    senhaAdministrador: ['', [Validators.required, Validators.minLength(8)]]
  });

  protected readonly loginForm = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });

  protected readonly atletaForm = this.fb.nonNullable.group({
    primeiroNome: ['', Validators.required],
    sobrenome: ['', Validators.required],
    genero: ['Masculino', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    telefone: [''],
    categoria: ['A', Validators.required]
  });

  protected readonly atletaFiltroForm = this.fb.nonNullable.group({
    nome: [''],
    status: [''],
    categoria: [''],
    genero: [''],
    ordenacao: ['ranking']
  });

  protected readonly atletaEdicaoForm = this.fb.nonNullable.group({
    primeiroNome: ['', Validators.required],
    sobrenome: ['', Validators.required],
    telefone: ['']
  });

  protected readonly torneioForm = this.fb.nonNullable.group({
    nome: ['', Validators.required],
    descricao: [''],
    tipo: ['Ladder', Validators.required],
    categoria: ['A', Validators.required],
    dataInicio: ['', Validators.required],
    dataFim: ['', Validators.required],
    pontuacaoVitoria: [10, Validators.required],
    pontuacaoDerrota: [-5, Validators.required],
    pontuacaoEmpate: [0, Validators.required],
    pontuacaoWO: [-20, Validators.required],
    permiteEmpate: [true, Validators.required]
  });

  protected readonly torneioOperacaoForm = this.fb.nonNullable.group({
    torneioId: ['', Validators.required],
    atletaId: ['', Validators.required]
  });

  protected readonly resultadoForm = this.fb.nonNullable.group({
    confrontoId: ['', Validators.required],
    tipoResultado: ['VitoriaAtletaA', Validators.required],
    justificativaWO: ['']
  });

  protected readonly rankingFiltroForm = this.fb.nonNullable.group({
    categoria: [''],
    genero: [''],
    periodoDias: ['']
  });

  protected readonly auditoriaFiltroForm = this.fb.nonNullable.group({
    entidade: [''],
    acao: [''],
    dataInicialUtc: [''],
    dataFinalUtc: ['']
  });

  protected authState: AuthState = {
    accessToken: '',
    tenantId: '',
    roles: [],
    email: ''
  };

  protected secaoAtual: SectionKey = 'atletas';
  protected carregando = false;
  protected mensagemCarregamento = '';

  protected atletas: Athlete[] = [];
  protected historicoAtleta: RankingHistory[] = [];
  protected atletaSelecionado: Athlete | null = null;

  protected torneios: Tournament[] = [];
  protected torneioSelecionado: Tournament | null = null;
  protected confrontos: Match[] = [];

  protected ranking: Athlete[] = [];
  protected auditorias: AuditEntry[] = [];

  ngOnInit(): void {
    const storedState = localStorage.getItem('cluberank.auth');
    if (!storedState) {
      return;
    }

    try {
      this.authState = JSON.parse(storedState) as AuthState;
      if (this.authState.accessToken) {
        void this.refreshWorkspace();
      }
    } catch {
      localStorage.removeItem('cluberank.auth');
    }
  }

  protected get autenticado(): boolean {
    return Boolean(this.authState.accessToken);
  }

  protected get resumoPerfis(): string {
    return this.authState.roles.join(', ') || 'Sem perfil';
  }

  protected get atletasAtivos(): Athlete[] {
    return this.atletas.filter((item) => item.status === 'Ativo');
  }

  protected get confrontosPendentes(): Match[] {
    return this.confrontos.filter((item) => item.status === 'Agendado');
  }

  protected async criarOrganizacao(): Promise<void> {
    if (this.onboardingForm.invalid) {
      this.onboardingForm.markAllAsTouched();
      return;
    }

    await this.execute('Criando organização', async () => {
      const payload = this.onboardingForm.getRawValue();
      await this.post('/api/organizacoes', payload, false);
      this.loginForm.patchValue({
        email: payload.email,
        password: payload.senhaAdministrador
      });
      this.snack('Organização criada. Você já pode entrar com o administrador.');
    });
  }

  protected async login(): Promise<void> {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    await this.execute('Autenticando', async () => {
      const response = await this.post<TokenResponse>('/api/auth/token', this.loginForm.getRawValue(), false);
      this.authState = {
        accessToken: response.accessToken,
        tenantId: response.tenantId,
        roles: response.roles,
        email: this.loginForm.controls.email.value
      };

      localStorage.setItem('cluberank.auth', JSON.stringify(this.authState));
      await this.refreshWorkspace();
      this.snack('Login realizado com sucesso.');
    });
  }

  protected logout(): void {
    this.authState = { accessToken: '', tenantId: '', roles: [], email: '' };
    this.atletas = [];
    this.historicoAtleta = [];
    this.atletaSelecionado = null;
    this.torneios = [];
    this.torneioSelecionado = null;
    this.confrontos = [];
    this.ranking = [];
    this.auditorias = [];
    localStorage.removeItem('cluberank.auth');
  }

  protected async criarAtleta(): Promise<void> {
    if (this.atletaForm.invalid) {
      this.atletaForm.markAllAsTouched();
      return;
    }

    await this.execute('Cadastrando atleta', async () => {
      await this.post('/api/atletas', this.atletaForm.getRawValue());
      this.atletaForm.reset({
        primeiroNome: '',
        sobrenome: '',
        genero: 'Masculino',
        email: '',
        telefone: '',
        categoria: 'A'
      });
      await this.refreshWorkspace();
      this.snack('Atleta cadastrado.');
    });
  }

  protected selecionarAtleta(atleta: Athlete): void {
    this.atletaSelecionado = atleta;
    const [primeiroNome, ...sobrenome] = atleta.nomeCompleto.split(' ');
    this.atletaEdicaoForm.patchValue({
      primeiroNome,
      sobrenome: sobrenome.join(' '),
      telefone: atleta.telefone ?? ''
    });
    void this.carregarHistorico(atleta.id);
  }

  protected async atualizarAtleta(): Promise<void> {
    if (!this.atletaSelecionado || this.atletaEdicaoForm.invalid) {
      this.atletaEdicaoForm.markAllAsTouched();
      return;
    }

    await this.execute('Atualizando atleta', async () => {
      await this.put(`/api/atletas/${this.atletaSelecionado!.id}`, this.atletaEdicaoForm.getRawValue());
      await this.refreshWorkspace();
      const atualizado = this.atletas.find((item) => item.id === this.atletaSelecionado?.id) ?? null;
      if (atualizado) {
        this.selecionarAtleta(atualizado);
      }
      this.snack('Atleta atualizado.');
    });
  }

  protected async alternarStatus(atleta: Athlete): Promise<void> {
    const acao = atleta.status === 'Ativo' ? 'inativar' : 'reativar';
    const texto = atleta.status === 'Ativo' ? 'Inativando atleta' : 'Reativando atleta';

    await this.execute(texto, async () => {
      await this.patch(`/api/atletas/${atleta.id}/${acao}`, {});
      await this.refreshWorkspace();
    });
  }

  protected async alterarCategoria(atleta: Athlete, categoria: string): Promise<void> {
    await this.execute('Atualizando categoria', async () => {
      await this.patch(`/api/atletas/${atleta.id}/categoria`, { categoria });
      await this.refreshWorkspace();
      if (this.atletaSelecionado?.id === atleta.id) {
        const atualizado = this.atletas.find((item) => item.id === atleta.id) ?? null;
        if (atualizado) {
          this.selecionarAtleta(atualizado);
        }
      }
    });
  }

  protected async aplicarFiltroAtletas(): Promise<void> {
    await this.execute('Carregando atletas', async () => {
      this.atletas = await this.get<Athlete[]>('/api/atletas', {
        pageNumber: 1,
        pageSize: 50,
        nome: this.atletaFiltroForm.controls.nome.value || null,
        status: this.atletaFiltroForm.controls.status.value || null,
        categoria: this.atletaFiltroForm.controls.categoria.value || null,
        genero: this.atletaFiltroForm.controls.genero.value || null,
        ordenacao: this.atletaFiltroForm.controls.ordenacao.value || 'ranking'
      });
    });
  }

  protected async criarTorneio(): Promise<void> {
    if (this.torneioForm.invalid) {
      this.torneioForm.markAllAsTouched();
      return;
    }

    await this.execute('Criando torneio', async () => {
      await this.post('/api/torneios', this.torneioForm.getRawValue());
      await this.carregarTorneios();
      this.snack('Torneio criado.');
    });
  }

  protected async selecionarTorneio(torneio: Tournament): Promise<void> {
    this.torneioSelecionado = torneio;
    this.torneioOperacaoForm.patchValue({ torneioId: torneio.id });
    this.resultadoForm.patchValue({ confrontoId: '' });
    await this.carregarConfrontos(torneio.id);
  }

  protected async adicionarAtletaAoTorneio(): Promise<void> {
    if (this.torneioOperacaoForm.invalid || !this.torneioSelecionado) {
      this.torneioOperacaoForm.markAllAsTouched();
      return;
    }

    await this.execute('Associando atleta ao torneio', async () => {
      await this.post(`/api/torneios/${this.torneioSelecionado!.id}/atletas`, {
        atletaId: this.torneioOperacaoForm.controls.atletaId.value
      });
      await this.carregarTorneios();
      await this.carregarConfrontos(this.torneioSelecionado!.id);
      this.snack('Atleta associado ao torneio.');
    });
  }

  protected async gerarConfrontos(): Promise<void> {
    if (!this.torneioSelecionado) {
      return;
    }

    await this.execute('Gerando confrontos', async () => {
      this.confrontos = await this.post<Match[]>(`/api/torneios/${this.torneioSelecionado!.id}/confrontos/gerar`, {});
      this.resultadoForm.patchValue({
        confrontoId: this.confrontosPendentes[0]?.id ?? ''
      });
      await this.carregarTorneios();
      await this.carregarAuditoria();
      this.snack('Confrontos gerados.');
    });
  }

  protected async registrarResultado(): Promise<void> {
    if (this.resultadoForm.invalid) {
      this.resultadoForm.markAllAsTouched();
      return;
    }

    await this.execute('Registrando resultado', async () => {
      await this.post('/api/resultados', this.resultadoForm.getRawValue());
      if (this.torneioSelecionado) {
        await this.carregarConfrontos(this.torneioSelecionado.id);
      }
      await this.carregarRanking();
      await this.carregarAuditoria();
      this.snack('Resultado registrado.');
    });
  }

  protected async carregarRankingFiltrado(): Promise<void> {
    await this.execute('Atualizando ranking', async () => {
      await this.carregarRanking();
    });
  }

  protected async aplicarDecaimento(): Promise<void> {
    await this.execute('Aplicando decaimento', async () => {
      await this.post('/api/rankings/decaimento', {});
      await this.carregarRanking();
      await this.carregarAuditoria();
      this.snack('Decaimento aplicado.');
    });
  }

  protected async carregarAuditoriaFiltrada(): Promise<void> {
    await this.execute('Carregando auditoria', async () => {
      await this.carregarAuditoria();
    });
  }

  protected mudarSecao(secao: SectionKey): void {
    this.secaoAtual = secao;
  }

  protected trackById(_index: number, item: { id: string }): string {
    return item.id;
  }

  private async refreshWorkspace(): Promise<void> {
    await this.execute('Sincronizando workspace', async () => {
      await Promise.all([
        this.carregarAtletas(),
        this.carregarTorneios(),
        this.carregarRanking(),
        this.carregarAuditoria()
      ]);
    });
  }

  private async carregarAtletas(): Promise<void> {
    this.atletas = await this.get<Athlete[]>('/api/atletas', {
      pageNumber: 1,
      pageSize: 50,
      ordenacao: this.atletaFiltroForm.controls.ordenacao.value || 'ranking'
    });
  }

  private async carregarHistorico(atletaId: string): Promise<void> {
    this.historicoAtleta = await this.get<RankingHistory[]>(`/api/atletas/${atletaId}/historico`);
  }

  private async carregarTorneios(): Promise<void> {
    this.torneios = await this.get<Tournament[]>('/api/torneios');
    if (this.torneioSelecionado) {
      this.torneioSelecionado = this.torneios.find((item) => item.id === this.torneioSelecionado?.id) ?? null;
    }
  }

  private async carregarConfrontos(torneioId: string): Promise<void> {
    this.confrontos = await this.get<Match[]>(`/api/torneios/${torneioId}/confrontos`);
    this.resultadoForm.patchValue({
      confrontoId: this.confrontosPendentes[0]?.id ?? this.resultadoForm.controls.confrontoId.value
    });
  }

  private async carregarRanking(): Promise<void> {
    this.ranking = await this.get<Athlete[]>('/api/rankings', {
      categoria: this.rankingFiltroForm.controls.categoria.value || null,
      genero: this.rankingFiltroForm.controls.genero.value || null,
      periodoDias: this.rankingFiltroForm.controls.periodoDias.value || null
    });
  }

  private async carregarAuditoria(): Promise<void> {
    this.auditorias = await this.get<AuditEntry[]>('/api/auditoria', {
      entidade: this.auditoriaFiltroForm.controls.entidade.value || null,
      acao: this.auditoriaFiltroForm.controls.acao.value || null,
      dataInicialUtc: this.toUtcStart(this.auditoriaFiltroForm.controls.dataInicialUtc.value),
      dataFinalUtc: this.toUtcEnd(this.auditoriaFiltroForm.controls.dataFinalUtc.value)
    });
  }

  private toUtcStart(value: string): string | null {
    return value ? `${value}T00:00:00Z` : null;
  }

  private toUtcEnd(value: string): string | null {
    return value ? `${value}T23:59:59Z` : null;
  }

  private async get<T>(path: string, params?: QueryParams): Promise<T> {
    let httpParams = new HttpParams();
    Object.entries(params ?? {}).forEach(([key, value]) => {
      if (value !== null && value !== undefined && value !== '') {
        httpParams = httpParams.set(key, String(value));
      }
    });

    return firstValueFrom(this.http.get<T>(`${this.apiUrl}${path}`, {
      headers: this.authHeaders(),
      params: httpParams
    }));
  }

  private async post<T>(path: string, body: unknown, authenticated = true): Promise<T> {
    return firstValueFrom(this.http.post<T>(`${this.apiUrl}${path}`, body, {
      headers: authenticated ? this.authHeaders() : new HttpHeaders({ 'Content-Type': 'application/json' })
    }));
  }

  private async put(path: string, body: unknown): Promise<void> {
    await firstValueFrom(this.http.put(`${this.apiUrl}${path}`, body, { headers: this.authHeaders() }));
  }

  private async patch(path: string, body: unknown): Promise<void> {
    await firstValueFrom(this.http.patch(`${this.apiUrl}${path}`, body, { headers: this.authHeaders() }));
  }

  private authHeaders(): HttpHeaders {
    let headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    if (this.authState.accessToken) {
      headers = headers.set('Authorization', `Bearer ${this.authState.accessToken}`);
    }

    return headers;
  }

  private async execute(label: string, action: () => Promise<void>): Promise<void> {
    this.carregando = true;
    this.mensagemCarregamento = label;

    try {
      await action();
    } catch (error) {
      this.snack(this.extractError(error));
    } finally {
      this.carregando = false;
      this.mensagemCarregamento = '';
    }
  }

  private snack(message: string): void {
    this.snackBar.open(message, 'Fechar', { duration: 4500 });
  }

  private extractError(error: unknown): string {
    const httpError = error as { error?: { title?: string; errors?: Record<string, string[]>; detail?: string } };
    const validation = httpError?.error?.errors;
    if (validation) {
      const firstGroup = Object.values(validation)[0];
      if (firstGroup?.length) {
        return firstGroup[0];
      }
    }

    return httpError?.error?.detail
      ?? httpError?.error?.title
      ?? 'Nao foi possivel concluir a operacao.';
  }
}

type SectionKey = 'atletas' | 'torneios' | 'ranking' | 'auditoria';

type QueryParams = Record<string, string | number | boolean | null | undefined>;

interface AuthState {
  accessToken: string;
  tenantId: string;
  roles: string[];
  email: string;
}

interface TokenResponse {
  accessToken: string;
  tenantId: string;
  roles: string[];
}

interface Athlete {
  id: string;
  nomeCompleto: string;
  genero: string;
  email: string;
  telefone?: string | null;
  status: string;
  categoria: string;
  pontuacaoAtual: number;
  dataAtualizacaoPontuacao: string;
  organizacaoId: string;
  dataCriacao: string;
}

interface RankingHistory {
  id: string;
  pontuacaoAntes: number;
  pontuacaoDepois: number;
  dataAtualizacao: string;
  motivo: string;
  confrontoId?: string | null;
  nomeAdversario?: string | null;
  resultado?: string | null;
}

interface Tournament {
  id: string;
  nome: string;
  descricao?: string | null;
  tipo: string;
  status: string;
  organizacaoId: string;
  categoria?: string | null;
  dataInicio?: string | null;
  dataFim?: string | null;
  pontuacaoVitoria: number;
  pontuacaoDerrota: number;
  pontuacaoEmpate: number;
  pontuacaoWO: number;
  permiteEmpate: boolean;
  quantidadeAtletas: number;
  dataCriacao: string;
}

interface Match {
  id: string;
  atletaAId: string;
  nomeAtletaA: string;
  atletaBId: string;
  nomeAtletaB: string;
  torneioId: string;
  status: string;
  resultado?: string | null;
  dataAgendamento: string;
  notas?: string | null;
}

interface AuditEntry {
  id: string;
  organizacaoId?: string | null;
  usuarioId?: string | null;
  entidade: string;
  entidadeId: string;
  acao: string;
  dadosAntes?: string | null;
  dadosDepois?: string | null;
  ip?: string | null;
  dataAcao: string;
}
