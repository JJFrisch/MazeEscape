import type { AuthChangeEvent, RealtimeChannel, Session } from '@supabase/supabase-js';
import { getSupabaseBrowserClient, SupabaseConfigError, SupabaseUrlError } from './client';
import { authStore } from './authStore.svelte';

interface InitializeSupabaseAuthOptions {
	onSignedIn?: (userId: string) => Promise<void> | void;
	onSignedOut?: () => Promise<void> | void;
}

export function initializeSupabaseAuth(options: InitializeSupabaseAuthOptions = {}): () => void {
	let supabase: ReturnType<typeof getSupabaseBrowserClient>;
	try {
		supabase = getSupabaseBrowserClient();
	} catch (err) {
		if (err instanceof SupabaseConfigError || err instanceof SupabaseUrlError) {
			// No backend in this build: play as a guest with local saves.
			console.warn(`[auth] ${err.message} Running in guest mode.`);
			authStore.handleAuthEvent('INITIAL_SESSION', null);
			void options.onSignedOut?.();
			return () => {};
		}
		throw err;
	}

	const handleSession = (event: AuthChangeEvent, session: Session | null) => {
		authStore.handleAuthEvent(event, session);
		if (session?.user?.id) {
			void options.onSignedIn?.(session.user.id);
		} else {
			void options.onSignedOut?.();
		}
	};

	const {
		data: { subscription }
	} = supabase.auth.onAuthStateChange((event, session) => {
		handleSession(event, session);
	});

	void supabase.auth.getSession().then(({ data }) => {
		handleSession('INITIAL_SESSION', data.session ?? null);
	});

	return () => {
		subscription.unsubscribe();
	};
}

export function subscribeToTable(
	channelName: string,
	table: string,
	onChange: () => void
): { channel: RealtimeChannel; cleanup: () => void } {
	const supabase = getSupabaseBrowserClient();
	const channel = supabase
		.channel(channelName)
		.on('postgres_changes', { event: '*', schema: 'public', table }, onChange)
		.subscribe();

	return {
		channel,
		cleanup: () => {
			void supabase.removeChannel(channel);
		}
	};
}