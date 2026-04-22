import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AdminValidacaoComponent } from './pages/admin-validacao/admin-validacao.component';
import { CategoriasComponent } from './pages/categorias/categorias.component';
import { ContratosComponent } from './pages/contratos/contratos.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { DocumentosCatalogoComponent } from './pages/documentos-catalogo/documentos-catalogo.component';
import { DocumentosExigidosComponent } from './pages/documentos-exigidos/documentos-exigidos.component';
import { FinanceiroComponent } from './pages/financeiro/financeiro.component';
import { EnvioNfComponent } from './pages/envio-nf/envio-nf.component';
import { FornecedoresComponent } from './pages/fornecedores/fornecedores.component';
import { LoginComponent } from './pages/login/login.component';
import { NotificacoesComponent } from './pages/notificacoes/notificacoes.component';
import { PortalFornecedorComponent } from './pages/portal-fornecedor/portal-fornecedor.component';
import { ResetPasswordComponent } from './pages/reset-password/reset-password.component';
import { UsuariosComponent } from './pages/usuarios/usuarios.component';
import { LayoutComponent } from './layout/layout.component';
import { AuthGuard } from './core/auth.guard';
import { RoleGuard } from './core/role.guard';

const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'redefinir-senha', component: ResetPasswordComponent },
  {
    path: '',
    component: LayoutComponent,
    canActivate: [AuthGuard],
    children: [
      { path: 'dashboard', component: DashboardComponent },
      { path: 'usuarios', component: UsuariosComponent, canActivate: [RoleGuard], data: { roles: ['Admin'] } },
      { path: 'fornecedores', component: FornecedoresComponent, canActivate: [RoleGuard], data: { roles: ['Admin', 'Financeiro'] } },
      { path: 'categorias', component: CategoriasComponent, canActivate: [RoleGuard], data: { roles: ['Admin'] } },
      { path: 'contratos', component: ContratosComponent },
      { path: 'documentos', component: DocumentosCatalogoComponent, canActivate: [RoleGuard], data: { roles: ['Admin'] } },
      { path: 'documentos-exigidos', component: DocumentosExigidosComponent, canActivate: [RoleGuard], data: { roles: ['Admin'] } },
      { path: 'portal-fornecedor', component: PortalFornecedorComponent, canActivate: [RoleGuard], data: { roles: ['Fornecedor'] } },
      { path: 'envio-nf', component: EnvioNfComponent, canActivate: [RoleGuard], data: { roles: ['Fornecedor'] } },
      { path: 'admin-validacao', component: AdminValidacaoComponent, canActivate: [RoleGuard], data: { roles: ['Admin', 'Financeiro'] } },
      { path: 'financeiro', component: FinanceiroComponent, canActivate: [RoleGuard], data: { roles: ['Admin', 'Financeiro'] } },
      { path: 'notificacoes', component: NotificacoesComponent }
    ]
  },
  { path: '**', redirectTo: 'dashboard' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
