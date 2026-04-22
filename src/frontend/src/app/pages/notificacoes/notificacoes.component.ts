import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../core/api.service';

@Component({
  selector: 'app-notificacoes',
  templateUrl: './notificacoes.component.html'
})
export class NotificacoesComponent implements OnInit {
  notificacoes: any[] = [];
  constructor(private readonly api: ApiService) {}
  ngOnInit(): void { this.api.get<any[]>('/notificacoes').subscribe((res) => this.notificacoes = res); }

  dataDaNotificacao(item: any): string | null {
    return item.dataHoraEnvio || item.dataHoraCriacao || null;
  }
}
