// ESLint flat config for the Theme Angular project.
// .editorconfig is enforced by Prettier (prettier --check, reads .editorconfig) and
// the line endings by .gitattributes. ESLint enforces the TypeScript, Angular and template rules.
// SonarCloud reads the ESLint report produced by `npm run lint:report` (see sonar-project.properties).
import eslint from '@eslint/js';
import tseslint from 'typescript-eslint';
import angular from 'angular-eslint';
import eslintConfigPrettier from 'eslint-config-prettier';

export default tseslint.config(
  {
    ignores: ['dist/', '.angular/', 'node_modules/', 'coverage/', 'eslint-report.json'],
  },

  // TypeScript sources
  {
    files: ['**/*.ts'],
    extends: [
      eslint.configs.recommended,
      tseslint.configs.recommendedTypeChecked,
      angular.configs.tsRecommended,
    ],
    languageOptions: {
      parserOptions: {
        projectService: true,
        tsconfigRootDir: import.meta.dirname,
      },
    },
    processor: angular.processInlineTemplates,
    rules: {
      // Angular selector prefix from angular.json ("prefix": "app")
      '@angular-eslint/component-selector': [
        'error',
        { type: 'element', prefix: 'app', style: 'kebab-case' },
      ],
      '@angular-eslint/directive-selector': [
        'error',
        { type: 'attribute', prefix: 'app', style: 'camelCase' },
      ],
      // Templates and styles live in external files (AGENTS.md ENG-UI-18)
      '@typescript-eslint/no-explicit-any': 'error',
      // Angular's Validators.* are static and never use `this`, so passing them unbound is safe
      '@typescript-eslint/unbound-method': ['error', { ignoreStatic: true }],
      '@typescript-eslint/no-floating-promises': 'error',
      '@typescript-eslint/consistent-type-definitions': ['error', 'interface'],
      eqeqeq: ['error', 'always'],
      curly: ['error', 'all'],
      'no-console': ['warn', { allow: ['warn', 'error'] }],
      // Unused parameters may be prefixed with _ (e.g. a signature kept for an interface)
      '@typescript-eslint/no-unused-vars': ['error', { argsIgnorePattern: '^_' }],
    },
  },

  // Angular component templates
  {
    files: ['**/*.html'],
    extends: [angular.configs.templateRecommended],
    rules: {
      // Templates must not use inline styles (styles live in external .css/.scss files)
      '@angular-eslint/template/no-inline-styles': ['error', { allowBindToStyle: 'dynamic' }],
    },
  },

  // Turn off stylistic rules that clash with Prettier formatting (.prettierrc)
  eslintConfigPrettier,
);
