{{/*
Expand the name of the chart.
*/}}
{{- define "clean-architecture.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
We truncate at 63 chars because some Kubernetes name fields are limited to this (by the DNS naming spec).
If release name contains chart name it will be used as a full name.
*/}}
{{- define "clean-architecture.fullname" -}}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := default .Chart.Name .Values.nameOverride }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}
{{- end }}

{{/*
Create chart name and version as used by the chart label.
*/}}
{{- define "clean-architecture.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Common labels
*/}}
{{- define "clean-architecture.labels" -}}
helm.sh/chart: {{ include "clean-architecture.chart" . }}
{{ include "clean-architecture.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Selector labels
*/}}
{{- define "clean-architecture.selectorLabels" -}}
app.kubernetes.io/name: {{ include "clean-architecture.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{/*
Accounts API fully qualified name
*/}}
{{- define "clean-architecture.accountsApi.fullname" -}}
{{- printf "%s-accounts-api" (include "clean-architecture.fullname" .) | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Accounts API labels
*/}}
{{- define "clean-architecture.accountsApi.labels" -}}
helm.sh/chart: {{ include "clean-architecture.chart" . }}
{{ include "clean-architecture.accountsApi.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Accounts API selector labels
*/}}
{{- define "clean-architecture.accountsApi.selectorLabels" -}}
app.kubernetes.io/name: {{ include "clean-architecture.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
app.kubernetes.io/component: accounts-api
{{- end }}

{{/*
Wallet App fully qualified name
*/}}
{{- define "clean-architecture.walletApp.fullname" -}}
{{- printf "%s-wallet-app" (include "clean-architecture.fullname" .) | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Wallet App labels
*/}}
{{- define "clean-architecture.walletApp.labels" -}}
helm.sh/chart: {{ include "clean-architecture.chart" . }}
{{ include "clean-architecture.walletApp.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Wallet App selector labels
*/}}
{{- define "clean-architecture.walletApp.selectorLabels" -}}
app.kubernetes.io/name: {{ include "clean-architecture.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
app.kubernetes.io/component: wallet-app
{{- end }}

{{/*
PostgreSQL fully qualified name
*/}}
{{- define "clean-architecture.postgresql.fullname" -}}
{{- printf "%s-postgresql" (include "clean-architecture.fullname" .) | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
PostgreSQL labels
*/}}
{{- define "clean-architecture.postgresql.labels" -}}
helm.sh/chart: {{ include "clean-architecture.chart" . }}
{{ include "clean-architecture.postgresql.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
PostgreSQL selector labels
*/}}
{{- define "clean-architecture.postgresql.selectorLabels" -}}
app.kubernetes.io/name: {{ include "clean-architecture.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
app.kubernetes.io/component: postgresql
{{- end }}

{{/*
PostgreSQL connection string
*/}}
{{- define "clean-architecture.postgresql.connectionString" -}}
{{- printf "Host=%s;Port=%d;Database=%s;Username=%s;Password=$(POSTGRES_PASSWORD)" (include "clean-architecture.postgresql.fullname" .) (int .Values.postgresql.port) .Values.postgresql.database .Values.postgresql.username }}
{{- end }}
