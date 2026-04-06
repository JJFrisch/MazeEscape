import type { RealtimeChannel, Session } from '@supabase/supabase-js';
import { getSupabaseBrowserClient } from './client';
import { authStore } from './authStore.svelte';

interface InitializeSupabaseAuthOptions {
	onSignedIn?: (userId: string) => Promise<void> | void;
	onSignedOut?: () => Promise<void> | void;
}

export function initializeSupabaseAuth(options: InitializeSupabaseAuthOptions = {}): () => void {
	const supabase = getSupabaseBrowserClient();

	const handleSession = (session: Session | null) => {
		authStore.setSession(session);
		if (session?.user?.id) {
			void options.onSignedIn?.(session.user.id);
		} else {
			void options.onSignedOut?.();
		}
	};

	const {
		data: { subscription }
	} = supabase.auth.onAuthStateChange((_event, session) => {
		handleSession(session);
	});

	void supabase.auth.getSession().then(({ data }) => {
		handleSession(data.session ?? null);
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