import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { LayoutComponent } from './layout/layout.component';
import { LoginComponent } from './pages/login/login.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { UsuariosComponent } from './pages/usuarios/usuarios.component';
import { FornecedoresComponent } from './pages/fornecedores/fornecedores.component';
import { CategoriasComponent } from './pages/categorias/categorias.component';
import { ContratosComponent } from './pages/contratos/contratos.component';
import { DocumentosCatalogoComponent } from './pages/documentos-catalogo/documentos-catalogo.component';
import { DocumentosExigidosComponent } from './pages/documentos-exigidos/documentos-exigidos.component';
import { PortalFornecedorComponent } from './pages/portal-fornecedor/portal-fornecedor.component';
import { AdminValidacaoComponent } from './pages/admin-validacao/admin-validacao.component';
import { FinanceiroComponent } from './pages/financeiro/financeiro.component';
import { EnvioNfComponent } from './pages/envio-nf/envio-nf.component';
import { NotificacoesComponent } from './pages/notificacoes/notificacoes.component';
import { ResetPasswordComponent } from './pages/reset-password/reset-password.component';
import { ApiErrorInterceptor } from './core/api-error.interceptor';
import { JwtInterceptor } from './core/jwt.interceptor';

@NgModule({
  declarations: [
    AppComponent,
    LayoutComponent,
    LoginComponent,
    DashboardComponent,
    UsuariosComponent,
    FornecedoresComponent,
    CategoriasComponent,
    ContratosComponent,
    DocumentosCatalogoComponent,
    DocumentosExigidosComponent,
    PortalFornecedorComponent,
    AdminValidacaoComponent,
    FinanceiroComponent,
    EnvioNfComponent,
    NotificacoesComponent,
    ResetPasswordComponent
  ],
  imports: [BrowserModule, HttpClientModule, FormsModule, ReactiveFormsModule, RouterModule, AppRoutingModule],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: ApiErrorInterceptor, multi: true }
  ],
  bootstrap: [AppComponent]
})
export class AppModule {}
