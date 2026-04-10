import { createClient, type SupabaseClient } from '@supabase/supabase-js';
import { PUBLIC_SUPABASE_URL, PUBLIC_SUPABASE_PUBLISHABLE_KEY } from '$env/static/public';

let browserClient: SupabaseClient | null = null;

export class SupabaseConfigError extends Error {
	constructor(message = 'Supabase public env vars were missing when this build was created.') {
		super(message);
		this.name = 'SupabaseConfigError';
	}
}

export class SupabaseUrlError extends Error {
	constructor(message = 'Supabase public URL is invalid.') {
		super(message);
		this.name = 'SupabaseUrlError';
	}
}

export interface SupabasePublicConfigDiagnostics {
	configured: boolean;
	url: string;
	hostname: string;
	validUrl: boolean;
}

export function getSupabasePublicConfigDiagnostics(): SupabasePublicConfigDiagnostics {
	const supabaseUrl = PUBLIC_SUPABASE_URL;

	if (!supabaseUrl) {
		return {
			configured: false,
			url: '',
			hostname: '',
			validUrl: false
		};
	}

	try {
		const parsedUrl = new URL(supabaseUrl);
		return {
			configured: true,
			url: parsedUrl.toString(),
			hostname: parsedUrl.hostname,
			validUrl: true
		};
	} catch {
		return {
			configured: true,
			url: supabaseUrl,
			hostname: '',
			validUrl: false
		};
	}
	}

export function getSupabaseBrowserClient(): SupabaseClient {
	const supabaseUrl = PUBLIC_SUPABASE_URL;
	const supabasePublishableKey = PUBLIC_SUPABASE_PUBLISHABLE_KEY;

	if (!supabaseUrl || !supabasePublishableKey) {
		throw new SupabaseConfigError(
			'Supabase public env vars were missing when this site was built. Re-run the web deploy after updating the GitHub Actions secrets.'
		);
	}

	let parsedSupabaseUrl: URL;
	try {
		parsedSupabaseUrl = new URL(supabaseUrl);
	} catch {
		throw new SupabaseUrlError(
			`Supabase public URL is invalid: ${supabaseUrl}. Update PUBLIC_SUPABASE_URL and re-run the web deploy.`
		);
	}

	if (!browserClient) {
		browserClient = createClient(parsedSupabaseUrl.toString(), supabasePublishableKey, {
			auth: {
				persistSession: true,
				autoRefreshToken: true,
				detectSessionInUrl: true
			}
		});
	}

	return browserClient;
}